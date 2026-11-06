using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.Grid
{
    public sealed class GridDataSourceOptions
    {
        public string? Url { get; set; }

        public string? DataFunction { get; set; }

        public TechHttpMethod Method { get; set; } = TechHttpMethod.Get;
    }
}
