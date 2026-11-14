namespace TechUI.Mvc.Infrastructure
{
    /// <summary>
    /// Computed aggregate returned next to the paged data.
    /// </summary>
    public sealed class TechGridSummaryValue
    {
        public string? Field { get; set; }

        public string Type { get; set; } = "sum";

        /// <summary>
        /// Aggregate result. Null when it cannot be computed (empty set or
        /// a non numeric field).
        /// </summary>
        public decimal? Value { get; set; }
    }
}
