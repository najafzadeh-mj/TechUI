using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.DropDownList
{
    public sealed class DropdownDataSourceOptions
    {
        public string? Url { get; set; }

        public string? DataFunction { get; set; }

        public TechHttpMethod Method { get; set; } = TechHttpMethod.Get;
    }
}
