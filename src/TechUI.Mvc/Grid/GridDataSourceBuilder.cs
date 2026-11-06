using Microsoft.AspNetCore.Mvc.Rendering;
using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.Grid
{
    public sealed class GridDataSourceBuilder
    {
        private readonly GridDataSourceOptions _options;
        private readonly IHtmlHelper _html;

        internal GridDataSourceBuilder(
            GridDataSourceOptions options,
            IHtmlHelper html)
        {
            _options = options;
            _html = html;
        }

        public GridDataSourceBuilder Url(string url)
        {
            _options.Url = url;
            return this;
        }

        public GridDataSourceBuilder Read(
            string action,
            string controller,
            string? area = null)
        {
            _options.Url = TechUrl.Action(_html, action, controller, area);
            return this;
        }

        public GridDataSourceBuilder Data(string functionName)
        {
            _options.DataFunction = functionName;
            return this;
        }

        public GridDataSourceBuilder Type(TechHttpMethod method)
        {
            _options.Method = method;
            return this;
        }
    }
}
