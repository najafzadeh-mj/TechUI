using System.Linq.Expressions;

namespace TechUI.Mvc.Grid
{
    internal static class GridExpressionHelper
    {
        /// <summary>
        /// Converts a member expression (optionally nested) to a dotted
        /// field path such as <c>Customer.FullName</c>.
        /// </summary>
        public static string GetFieldPath(Expression expression)
        {
            var body = expression is UnaryExpression unary
                ? unary.Operand
                : expression;

            var parts = new List<string>();
            var current = body;

            while (current is MemberExpression member)
            {
                parts.Insert(0, member.Member.Name);
                current = member.Expression;
            }

            if (parts.Count == 0 || current is not ParameterExpression)
            {
                throw new InvalidOperationException("Invalid field expression.");
            }

            return string.Join(".", parts);
        }
    }
}
