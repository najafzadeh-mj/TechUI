namespace TechUI.Mvc.Infrastructure
{
    /// <summary>
    /// A grouping level requested by the grid client.
    /// </summary>
    public sealed class TechGridGroup
    {
        public string Field { get; set; } = string.Empty;

        public string? Title { get; set; }
    }
}
