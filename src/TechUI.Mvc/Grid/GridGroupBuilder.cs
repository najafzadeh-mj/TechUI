using System.Linq.Expressions;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Declares row grouping levels. The client groups the current page rows
    /// by these fields in declaration order.
    /// </summary>
    public sealed class GridGroupBuilder<TModel>
    {
        private readonly IList<GridGroup> _groups;

        internal GridGroupBuilder(IList<GridGroup> groups)
        {
            _groups = groups;
        }

        public GridGroupBuilder<TModel> By<TValue>(
            Expression<Func<TModel, TValue>> expression,
            string? title = null)
        {
            ArgumentNullException.ThrowIfNull(expression);

            return By(GridExpressionHelper.GetFieldPath(expression.Body), title);
        }

        public GridGroupBuilder<TModel> By(string field, string? title = null)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ArgumentException(
                    "Group field cannot be empty.",
                    nameof(field));
            }

            _groups.Add(new GridGroup
            {
                Field = field.Trim(),
                Title = title
            });

            return this;
        }
    }
}
