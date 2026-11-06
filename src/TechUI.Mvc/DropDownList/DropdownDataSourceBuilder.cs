using Microsoft.AspNetCore.Mvc.Rendering;
using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.DropDownList
{
    public sealed class DropdownDataSourceBuilder
    {
        private readonly DropdownDataSourceOptions _options;
        private readonly IHtmlHelper _html;

        internal DropdownDataSourceBuilder(
            DropdownDataSourceOptions options,
            IHtmlHelper html)
        {
            _options = options;
            _html = html;
        }

        public DropdownDataSourceBuilder Url(string url)
        {
            _options.Url = url;
            return this;
        }

        public DropdownDataSourceBuilder Read(
            string action,
            string controller,
            string? area = null)
        {
            _options.Url = TechUrl.Action(_html, action, controller, area);
            return this;
        }

        public DropdownDataSourceBuilder Data(string functionName)
        {
            _options.DataFunction = functionName;
            return this;
        }

        public DropdownDataSourceBuilder Type(TechHttpMethod method)
        {
            _options.Method = method;
            return this;
        }
    }
}
