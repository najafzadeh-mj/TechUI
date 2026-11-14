using System.Linq.Expressions;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Declares aggregates rendered in the Grid summary footer. Values are
    /// computed by the server over the filtered data (all pages).
    /// </summary>
    public sealed class GridSummaryBuilder<TModel>
    {
        private readonly IList<GridSummaryItem> _items;

        internal GridSummaryBuilder(IList<GridSummaryItem> items)
        {
            _items = items;
        }

        /// <summary>
        /// Row count of the filtered data, or non-null count for a field.
        /// </summary>
        public GridSummaryBuilder<TModel> Count(
            string? field = null,
            string? label = null)
        {
            _items.Add(new GridSummaryItem
            {
                Field = string.IsNullOrWhiteSpace(field) ? null : field.Trim(),
                Type = GridSummaryType.Count,
                Label = label
            });

            return this;
        }

        public GridSummaryBuilder<TModel> Sum<TValue>(
            Expression<Func<TModel, TValue>> expression,
            string? label = null)
        {
            return Add(expression, GridSummaryType.Sum, label);
        }

        public GridSummaryBuilder<TModel> Average<TValue>(
            Expression<Func<TModel, TValue>> expression,
            string? label = null)
        {
            return Add(expression, GridSummaryType.Average, label);
        }

        public GridSummaryBuilder<TModel> Min<TValue>(
            Expression<Func<TModel, TValue>> expression,
            string? label = null)
        {
            return Add(expression, GridSummaryType.Min, label);
        }

        public GridSummaryBuilder<TModel> Max<TValue>(
            Expression<Func<TModel, TValue>> expression,
            string? label = null)
        {
            return Add(expression, GridSummaryType.Max, label);
        }

        private GridSummaryBuilder<TModel> Add<TValue>(
            Expression<Func<TModel, TValue>> expression,
            GridSummaryType type,
            string? label)
        {
            ArgumentNullException.ThrowIfNull(expression);

            _items.Add(new GridSummaryItem
            {
                Field = GridExpressionHelper.GetFieldPath(expression.Body),
                Type = type,
                Label = label
            });

            return this;
        }
    }
}
