namespace TechUI.Mvc.Grid
{
    public sealed class GridSummaryItem
    {
        /// <summary>
        /// Field path to aggregate; null for an overall row count.
        /// </summary>
        public string? Field { get; set; }

        public GridSummaryType Type { get; set; }

        public string? Label { get; set; }
    }
}
