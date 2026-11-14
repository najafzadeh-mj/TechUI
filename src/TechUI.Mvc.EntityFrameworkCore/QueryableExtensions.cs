using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.Grid
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Applies TechUI filtering, sorting and paging to a query.
        /// </summary>
        /// <param name="allowedFields">
        /// Optional whitelist of property names that clients may sort or filter by.
        /// When omitted, any public property can be used.
        /// </param>
        public static async Task<TechGridResult<T>> ToTechGridResultAsync<T>(
            this IQueryable<T> query,
            TechGridRequest request,
            IEnumerable<string>? allowedFields = null)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(request);

            var page = request.Page > 0 ? request.Page : 1;
            var pageSize = Math.Clamp(
                request.PageSize,
                1,
                TechGridRequest.MaxPageSize);

            var allowed = BuildAllowedSet(allowedFields);

            query = ApplyFilters(query, request.Filter, allowed);

            var sorts = (request.Sorts ?? new List<TechGridSort>())
                .Where(sort => !string.IsNullOrWhiteSpace(sort.Field))
                .Where(sort => IsAllowed(sort.Field, allowed))
                .ToList();

            var groupFields = ResolveGroupFields<T>(request.Groups, allowed);

            if (groupFields.Count > 0)
            {
                GroupedPage<T>? grouped = null;

                try
                {
                    grouped = await BuildGroupedPageAsync(
                        query, sorts, groupFields, page, pageSize);
                }
                catch (Exception ex) when (
                    ex is InvalidOperationException or
                        NotSupportedException or
                        ArgumentException or
                        TargetInvocationException)
                {
                    // The provider cannot translate the grouping query;
                    // fall back to row paging. The client still groups the
                    // rows of the current page.
                    grouped = null;
                }

                if (grouped != null)
                {
                    var groupedSummary = await ComputeSummaryAsync(
                        query, request.Summary, allowed, grouped.RowTotal);

                    return new TechGridResult<T>
                    {
                        Data = grouped.Data,
                        Total = grouped.Total,
                        RowTotal = grouped.RowTotal,
                        Grouped = true,
                        Page = page,
                        PageSize = pageSize,
                        Summary = groupedSummary
                    };
                }
            }

            if (sorts.Count > 0)
            {
                query = query.ApplySorts(sorts);
            }
            else if (!string.IsNullOrWhiteSpace(request.SortField) &&
                     IsAllowed(request.SortField, allowed))
            {
                query = query.OrderByProperty(
                    request.SortField,
                    string.Equals(
                        request.SortDir,
                        "desc",
                        StringComparison.OrdinalIgnoreCase));
            }

            var pagedQuery = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            int total;
            List<T> data;

            if (query.Provider is IAsyncQueryProvider)
            {
                total = await EntityFrameworkQueryableExtensions.CountAsync(query);
                data = await EntityFrameworkQueryableExtensions.ToListAsync(pagedQuery);
            }
            else
            {
                total = query.Count();
                data = pagedQuery.ToList();
            }

            var summary = await ComputeSummaryAsync(
                query,
                request.Summary,
                allowed,
                total);

            return new TechGridResult<T>
            {
                Data = data,
                Total = total,
                Page = page,
                PageSize = pageSize,
                Summary = summary
            };
        }

        // -------------------------------------------------------
        // Server side grouping
        // -------------------------------------------------------

        private sealed class GroupedPage<T>
        {
            public List<T> Data { get; init; } = new();

            public int Total { get; init; }

            public int RowTotal { get; init; }
        }

        private static List<string> ResolveGroupFields<T>(
            List<TechGridGroup>? groups,
            HashSet<string>? allowed)
        {
            var fields = new List<string>();

            if (groups is not { Count: > 0 })
            {
                return fields;
            }

            foreach (var group in groups)
            {
                // Either every level can be grouped on the server or none
                // of them: a partial grouping would scatter the rows of the
                // remaining levels on the client.
                if (group == null ||
                    string.IsNullOrWhiteSpace(group.Field) ||
                    !IsAllowed(group.Field, allowed))
                {
                    return new List<string>();
                }

                var parameter = Expression.Parameter(typeof(T), "x");
                var property = ResolveProperty(parameter, group.Field);

                if (property == null || !IsGroupableType(property.Type))
                {
                    return new List<string>();
                }

                if (!fields.Contains(group.Field, StringComparer.OrdinalIgnoreCase))
                {
                    fields.Add(group.Field);
                }
            }

            return fields;
        }

        private static bool IsGroupableType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            return type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(Guid) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(DateOnly) ||
                type == typeof(TimeOnly) ||
                type == typeof(TimeSpan);
        }

        private static async Task<GroupedPage<T>?> BuildGroupedPageAsync<T>(
            IQueryable<T> query,
            IReadOnlyList<TechGridSort> sorts,
            IReadOnlyList<string> groupFields,
            int page,
            int pageSize)
        {
            var asynchronous = query.Provider is IAsyncQueryProvider;
            var field = groupFields[0];
            var skip = (page - 1) * pageSize;

            var rowTotal = asynchronous
                ? await EntityFrameworkQueryableExtensions.CountAsync(query)
                : query.Count();

            var total = await CountKeysAsync(query, field, asynchronous);
            var keys = await SelectKeysAsync(
                query, field, skip, pageSize, asynchronous);

            if (keys.Count == 0)
            {
                return new GroupedPage<T>
                {
                    Total = total,
                    RowTotal = rowTotal
                };
            }

            var pageQuery = FilterByKeys(query, field, keys);
            var ordered = ApplyGroupedOrdering(pageQuery, groupFields, sorts);

            var data = asynchronous
                ? await EntityFrameworkQueryableExtensions.ToListAsync(ordered)
                : ordered.ToList();

            return new GroupedPage<T>
            {
                Data = data,
                Total = total,
                RowTotal = rowTotal
            };
        }

        private static async Task<int> CountKeysAsync<T>(
            IQueryable<T> query,
            string field,
            bool asynchronous)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolveProperty(parameter, field)!;
            var lambda = Expression.Lambda(property, parameter);

            var method = typeof(QueryableExtensions)
                .GetMethod(
                    nameof(CountKeysTypedAsync),
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T), property.Type);

            var task = (Task<int>)method.Invoke(
                null,
                new object?[] { query, lambda, asynchronous })!;

            return await task;
        }

        private static async Task<int> CountKeysTypedAsync<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> selector,
            bool asynchronous)
        {
            var keys = query.Select(selector).Distinct();

            return asynchronous
                ? await EntityFrameworkQueryableExtensions.CountAsync(keys)
                : keys.Count();
        }

        private static async Task<List<object?>> SelectKeysAsync<T>(
            IQueryable<T> query,
            string field,
            int skip,
            int take,
            bool asynchronous)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolveProperty(parameter, field)!;
            var lambda = Expression.Lambda(property, parameter);

            var method = typeof(QueryableExtensions)
                .GetMethod(
                    nameof(SelectKeysTypedAsync),
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T), property.Type);

            var task = (Task<List<object?>>)method.Invoke(
                null,
                new object?[] { query, lambda, skip, take, asynchronous })!;

            return await task;
        }

        private static async Task<List<object?>> SelectKeysTypedAsync<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> selector,
            int skip,
            int take,
            bool asynchronous)
        {
            var keys = query
                .Select(selector)
                .Distinct()
                .OrderBy(key => key)
                .Skip(skip)
                .Take(take);

            var list = asynchronous
                ? await EntityFrameworkQueryableExtensions.ToListAsync(keys)
                : keys.ToList();

            return list.Cast<object?>().ToList();
        }

        private static IQueryable<T> FilterByKeys<T>(
            IQueryable<T> query,
            string field,
            List<object?> keys)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolveProperty(parameter, field)!;

            var method = typeof(QueryableExtensions)
                .GetMethod(
                    nameof(FilterByKeysTyped),
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)method.Invoke(
                null,
                new object?[] { query, field, keys })!;
        }

        private static IQueryable<T> FilterByKeysTyped<T, TKey>(
            IQueryable<T> query,
            string field,
            List<object?> keys)
        {
            var values = new List<TKey>(keys.Count);

            foreach (var key in keys)
            {
                values.Add(key == null ? default! : (TKey)key);
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolveProperty(parameter, field)!;
            var contains = Expression.Call(
                Expression.Constant(values, typeof(List<TKey>)),
                typeof(List<TKey>)
                    .GetMethod(nameof(List<TKey>.Contains), new[] { typeof(TKey) })!,
                property);

            return query.Where(
                Expression.Lambda<Func<T, bool>>(contains, parameter));
        }

        private static IQueryable<T> ApplyGroupedOrdering<T>(
            IQueryable<T> source,
            IReadOnlyList<string> groupFields,
            IReadOnlyList<TechGridSort> sorts)
        {
            IOrderedQueryable<T>? ordered = null;

            foreach (var field in groupFields)
            {
                ordered = ApplyPropertyOrder(
                    source, ordered, field, descending: false);
            }

            return ApplySorts(source, sorts, ordered);
        }

        private static IOrderedQueryable<T> ApplyPropertyOrder<T>(
            IQueryable<T> source,
            IOrderedQueryable<T>? ordered,
            string field,
            bool descending)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolveProperty(parameter, field);

            if (property == null)
            {
                return ordered ?? source.OrderBy(_ => 0);
            }

            var lambda = Expression.Lambda(property, parameter);

            var methodName = ordered == null
                ? descending ? "OrderByDescending" : "OrderBy"
                : descending ? "ThenByDescending" : "ThenBy";

            var method = typeof(Queryable)
                .GetMethods()
                .First(m =>
                    m.Name == methodName &&
                    m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IOrderedQueryable<T>)method.Invoke(
                null,
                new object?[] { ordered ?? source, lambda })!;
        }

        // -------------------------------------------------------
        // Summary aggregates
        // -------------------------------------------------------

        private static async Task<List<TechGridSummaryValue>> ComputeSummaryAsync<T>(
            IQueryable<T> query,
            List<TechGridSummary>? specs,
            HashSet<string>? allowed,
            int total)
        {
            var values = new List<TechGridSummaryValue>();

            if (specs is not { Count: > 0 })
            {
                return values;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var async = query.Provider is IAsyncQueryProvider;

            foreach (var spec in specs)
            {
                var type = NormalizeSummaryType(spec?.Type);
                var item = new TechGridSummaryValue
                {
                    Field = spec?.Field,
                    Type = type ?? "sum"
                };

                if (type == null)
                {
                    values.Add(item);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(spec!.Field))
                {
                    if (type == "count") item.Value = total;

                    values.Add(item);
                    continue;
                }

                if (!IsAllowed(spec.Field, allowed))
                {
                    values.Add(item);
                    continue;
                }

                var property = ResolveProperty(parameter, spec.Field);

                if (property == null)
                {
                    values.Add(item);
                    continue;
                }

                if (type == "count")
                {
                    item.Value = await CountFieldAsync(
                        query, property, parameter, async);

                    values.Add(item);
                    continue;
                }

                var selector = BuildDecimalSelector<T>(property, parameter);

                if (selector == null)
                {
                    values.Add(item);
                    continue;
                }

                item.Value = await AggregateAsync(query, selector, type, async);

                values.Add(item);
            }

            return values;
        }

        private static string? NormalizeSummaryType(string? type)
        {
            return (type ?? "sum").Trim().ToLowerInvariant() switch
            {
                "count" => "count",
                "sum" => "sum",
                "avg" or "average" => "average",
                "min" => "min",
                "max" => "max",
                _ => null
            };
        }

        private static Expression<Func<T, decimal?>>? BuildDecimalSelector<T>(
            Expression property,
            ParameterExpression parameter)
        {
            var type = Nullable.GetUnderlyingType(property.Type) ?? property.Type;

            if (type != typeof(decimal) &&
                type != typeof(double) &&
                type != typeof(float) &&
                type != typeof(int) &&
                type != typeof(long) &&
                type != typeof(short) &&
                type != typeof(byte) &&
                type != typeof(uint) &&
                type != typeof(ulong) &&
                type != typeof(ushort) &&
                type != typeof(sbyte))
            {
                return null;
            }

            var body = Expression.Convert(property, typeof(decimal?));

            return Expression.Lambda<Func<T, decimal?>>(body, parameter);
        }

        private static async Task<decimal?> CountFieldAsync<T>(
            IQueryable<T> query,
            Expression property,
            ParameterExpression parameter,
            bool async)
        {
            if (!SupportsNull(property.Type))
            {
                return async
                    ? await EntityFrameworkQueryableExtensions.CountAsync(query)
                    : query.Count();
            }

            var predicate = Expression.Lambda<Func<T, bool>>(
                Expression.NotEqual(
                    property,
                    Expression.Constant(null, property.Type)),
                parameter);

            return async
                ? await EntityFrameworkQueryableExtensions.CountAsync(
                    query, predicate)
                : query.Count(predicate);
        }

        private static async Task<decimal?> AggregateAsync<T>(
            IQueryable<T> query,
            Expression<Func<T, decimal?>> selector,
            string type,
            bool async)
        {
            var projected = query.Select(selector);

            return type switch
            {
                "sum" => async
                    ? await EntityFrameworkQueryableExtensions.SumAsync(projected)
                    : projected.Sum(),
                "average" => async
                    ? await EntityFrameworkQueryableExtensions.AverageAsync(projected)
                    : projected.Average(),
                "min" => async
                    ? await EntityFrameworkQueryableExtensions.MinAsync(projected)
                    : projected.Min(),
                _ => async
                    ? await EntityFrameworkQueryableExtensions.MaxAsync(projected)
                    : projected.Max()
            };
        }

        // -------------------------------------------------------
        // Filtering
        // -------------------------------------------------------

        private static IQueryable<T> ApplyFilters<T>(
            IQueryable<T> source,
            TechGridFilterGroup? group,
            HashSet<string>? allowed)
        {
            if (group?.Filters is not { Count: > 0 })
            {
                return source;
            }

            var useOr = string.Equals(
                group.Logic,
                "or",
                StringComparison.OrdinalIgnoreCase);

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            foreach (var filter in group.Filters)
            {
                if (filter == null ||
                    string.IsNullOrWhiteSpace(filter.Field) ||
                    !IsAllowed(filter.Field, allowed))
                {
                    continue;
                }

                var expression = BuildFilterExpression(parameter, filter);

                if (expression == null)
                {
                    continue;
                }

                body = body == null
                    ? expression
                    : useOr
                        ? Expression.OrElse(body, expression)
                        : Expression.AndAlso(body, expression);
            }

            if (body == null)
            {
                return source;
            }

            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

            return source.Where(lambda);
        }

        private static Expression? BuildFilterExpression(
            ParameterExpression parameter,
            TechGridFilter filter)
        {
            var property = ResolveProperty(parameter, filter.Field);

            if (property == null)
            {
                return null;
            }

            var propertyType = Nullable.GetUnderlyingType(property.Type)
                ?? property.Type;

            var operation = (filter.Operator ?? "contains")
                .Trim()
                .ToLowerInvariant();

            if (operation == "isnull")
            {
                return SupportsNull(property.Type)
                    ? Expression.Equal(
                        property,
                        Expression.Constant(null, property.Type))
                    : Expression.Constant(false);
            }

            if (operation == "isnotnull")
            {
                return SupportsNull(property.Type)
                    ? Expression.NotEqual(
                        property,
                        Expression.Constant(null, property.Type))
                    : Expression.Constant(true);
            }

            Expression left = property;

            if (property.Type != propertyType)
            {
                left = Expression.Convert(property, propertyType);
            }

            if (operation is "in" or "notin")
            {
                return BuildInExpression(left, propertyType, filter.Value, operation == "notin");
            }

            if (operation is "contains" or "notcontains" or
                "startswith" or "endswith")
            {
                if (propertyType != typeof(string))
                {
                    return null;
                }

                var text = filter.Value ?? string.Empty;

                Expression call = operation switch
                {
                    "contains" or "notcontains" => Expression.Call(
                        left,
                        typeof(string).GetMethod(
                            nameof(string.Contains),
                            new[] { typeof(string) })!,
                        Expression.Constant(text)),

                    "startswith" => Expression.Call(
                        left,
                        typeof(string).GetMethod(
                            nameof(string.StartsWith),
                            new[] { typeof(string) })!,
                        Expression.Constant(text)),

                    _ => Expression.Call(
                        left,
                        typeof(string).GetMethod(
                            nameof(string.EndsWith),
                            new[] { typeof(string) })!,
                        Expression.Constant(text))
                };

                var notNull = Expression.NotEqual(
                    left,
                    Expression.Constant(null, typeof(string)));

                var hasValue = operation == "notcontains"
                    ? Expression.Not(call)
                    : call;

                return operation == "notcontains"
                    ? Expression.OrElse(
                        Expression.Equal(
                            left,
                            Expression.Constant(null, typeof(string))),
                        hasValue)
                    : Expression.AndAlso(notNull, hasValue);
            }

            if (string.IsNullOrEmpty(filter.Value))
            {
                return null;
            }

            if (!TryConvertValue(filter.Value, propertyType, out var value))
            {
                return null;
            }

            var constant = Expression.Constant(value, propertyType);

            try
            {
                return operation switch
                {
                    "eq" => Expression.Equal(left, constant),
                    "neq" => Expression.NotEqual(left, constant),
                    "gt" => Expression.GreaterThan(left, constant),
                    "gte" => Expression.GreaterThanOrEqual(left, constant),
                    "lt" => Expression.LessThan(left, constant),
                    "lte" => Expression.LessThanOrEqual(left, constant),
                    _ => null
                };
            }
            catch (InvalidOperationException)
            {
                // The operator is not defined for this property type.
                return null;
            }
        }

        private static Expression? BuildInExpression(
            Expression left,
            Type propertyType,
            string? value,
            bool negate)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var values = value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(item => TryConvertValue(item, propertyType, out var converted)
                    ? converted
                    : null)
                .Where(item => item != null)
                .ToList();

            if (values.Count == 0)
            {
                return null;
            }

            Expression? body = null;

            foreach (var item in values)
            {
                var equals = Expression.Equal(
                    left,
                    Expression.Constant(item, propertyType));

                body = body == null
                    ? equals
                    : Expression.OrElse(body, equals);
            }

            return body == null
                ? null
                : negate
                    ? Expression.Not(body)
                    : body;
        }

        // -------------------------------------------------------
        // Dynamic sorting
        // -------------------------------------------------------

        private static IQueryable<T> OrderByProperty<T>(
            this IQueryable<T> source,
            string propertyName,
            bool descending)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = ResolveProperty(parameter, propertyName);

            if (property == null)
            {
                return source;
            }

            var lambda = Expression.Lambda(property, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";

            var method = typeof(Queryable)
                .GetMethods()
                .First(m =>
                    m.Name == methodName &&
                    m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.Type);

            return (IQueryable<T>)method.Invoke(null, new object[] { source, lambda })!;
        }

        private static IQueryable<T> ApplySorts<T>(
            this IQueryable<T> source,
            IReadOnlyList<TechGridSort> sorts,
            IOrderedQueryable<T>? ordered = null)
        {
            var appliedCount = ordered == null ? 0 : 1;

            foreach (var sort in sorts)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = ResolveProperty(parameter, sort.Field);

                if (property == null)
                {
                    continue;
                }

                var lambda = Expression.Lambda(property, parameter);

                var descending = string.Equals(
                    sort.Dir,
                    "desc",
                    StringComparison.OrdinalIgnoreCase);

                var methodName = appliedCount == 0
                    ? descending ? "OrderByDescending" : "OrderBy"
                    : descending ? "ThenByDescending" : "ThenBy";

                var method = typeof(Queryable)
                    .GetMethods()
                    .First(m =>
                        m.Name == methodName &&
                        m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.Type);

                ordered = (IOrderedQueryable<T>)method.Invoke(
                    null,
                    new object[] { ordered ?? source, lambda })!;

                appliedCount++;
            }

            return ordered ?? source;
        }

        // -------------------------------------------------------
        // Helpers
        // -------------------------------------------------------

        private static HashSet<string>? BuildAllowedSet(IEnumerable<string>? allowedFields)
        {
            return allowedFields?
                .Where(field => !string.IsNullOrWhiteSpace(field))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsAllowed(string field, HashSet<string>? allowed)
        {
            return allowed == null || allowed.Contains(field);
        }

        private static bool SupportsNull(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        private static Expression? ResolveProperty(
            Expression instance,
            string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            Expression current = instance;

            foreach (var segment in path.Split(
                         '.',
                         StringSplitOptions.RemoveEmptyEntries |
                         StringSplitOptions.TrimEntries))
            {
                if (string.IsNullOrWhiteSpace(segment))
                {
                    return null;
                }

                MemberInfo? member = current.Type.GetProperty(
                        segment,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.IgnoreCase)
                    ?? (MemberInfo?)current.Type.GetField(
                        segment,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.IgnoreCase);

                if (member == null)
                {
                    return null;
                }

                current = Expression.MakeMemberAccess(current, member);
            }

            return current;
        }

        private static bool TryConvertValue(
            string raw,
            Type type,
            out object? value)
        {
            value = null;

            if (type == typeof(string))
            {
                value = raw;
                return true;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (type == typeof(bool))
            {
                if (bool.TryParse(raw, out var boolean))
                {
                    value = boolean;
                    return true;
                }

                if (raw == "0" || raw == "1")
                {
                    value = raw == "1";
                    return true;
                }

                return false;
            }

            if (type == typeof(Guid))
            {
                if (Guid.TryParse(raw, out var guid))
                {
                    value = guid;
                    return true;
                }

                return false;
            }

            if (type.IsEnum)
            {
                try
                {
                    value = Enum.Parse(type, raw, ignoreCase: true);
                    return true;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            if (type == typeof(DateTime))
            {
                if (DateTime.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var invariantDate) ||
                    DateTime.TryParse(
                        raw,
                        CultureInfo.CurrentCulture,
                        DateTimeStyles.None,
                        out invariantDate))
                {
                    value = invariantDate;
                    return true;
                }

                return false;
            }

            if (type == typeof(DateTimeOffset))
            {
                if (DateTimeOffset.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var offset))
                {
                    value = offset;
                    return true;
                }

                return false;
            }

            if (type == typeof(DateOnly))
            {
                if (DateOnly.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                {
                    value = date;
                    return true;
                }

                return false;
            }

            if (type == typeof(TimeOnly))
            {
                if (TimeOnly.TryParse(
                        raw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var time))
                {
                    value = time;
                    return true;
                }

                return false;
            }

            if (type == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(raw, CultureInfo.InvariantCulture, out var span))
                {
                    value = span;
                    return true;
                }

                return false;
            }

            try
            {
                value = Convert.ChangeType(raw, type, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex) when (
                ex is FormatException or InvalidCastException or OverflowException)
            {
                return false;
            }
        }
    }
}
