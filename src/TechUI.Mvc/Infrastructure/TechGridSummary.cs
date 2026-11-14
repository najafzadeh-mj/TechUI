namespace TechUI.Mvc.Infrastructure
{
    /// <summary>
    /// Aggregate requested by a TechUI Grid summary row.
    /// </summary>
    public sealed class TechGridSummary
    {
        /// <summary>
        /// Field to aggregate. Leave empty for an overall row count.
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// One of: count, sum, average, min, max.
        /// </summary>
        public string Type { get; set; } = "sum";
    }
}
