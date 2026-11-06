using System.Linq;
using System.Linq.Expressions;

namespace TechUI.Mvc.Grid
{
    public sealed class GridColumnFactory<TModel>
    {
        private readonly IList<GridColumn> _columns;

        internal GridColumnFactory(IList<GridColumn> columns)
        {
            _columns = columns;
        }

        public GridColumnBuilder Bound<TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var body = expression.Body is UnaryExpression unary
                ? unary.Operand
                : expression.Body;

            var field = BuildFieldPath(body);

            var column = new GridColumn
            {
                Key = BuildKey(field),
                Field = field,
                Title = field[(field.LastIndexOf('.') + 1)..],
                DataType = GetDataType(typeof(TValue))
            };

            _columns.Add(column);

            return new GridColumnBuilder(column);
        }

        public GridColumnBuilder Template(string template)
        {
            var column = new GridColumn
            {
                Key = BuildKey($"template_{_columns.Count + 1}"),
                IsTemplate = true,
                Template = template,
                Sortable = false,
                Filterable = false
            };

            _columns.Add(column);

            return new GridColumnBuilder(column);
        }

        private static string BuildFieldPath(Expression expression)
        {
            var parts = new List<string>();
            var current = expression;

            while (current is MemberExpression member)
            {
                parts.Insert(0, member.Member.Name);
                current = member.Expression;
            }

            if (parts.Count == 0 || current is not ParameterExpression)
            {
                throw new InvalidOperationException("Invalid column expression.");
            }

            return string.Join(".", parts);
        }

        private static string? GetDataType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(string)) return "string";
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(DateOnly)) return "date";
            if (type == typeof(TimeOnly) || type == typeof(TimeSpan)) return "time";
            if (type == typeof(Guid)) return "guid";
            if (type.IsEnum) return "enum";
            if (type.IsPrimitive || type == typeof(decimal)) return "number";

            return null;
        }

        private string BuildKey(string baseKey)
        {
            var key = baseKey;
            var suffix = 2;

            while (_columns.Any(column => column.Key == key))
            {
                key = $"{baseKey}_{suffix++}";
            }

            return key;
        }
    }
}
