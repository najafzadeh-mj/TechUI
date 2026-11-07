using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Body posted by the built-in grid chat panel. The endpoint is fully
    /// provider-agnostic: TechUI ships no AI dependency.
    /// </summary>
    public sealed class TechGridChatRequest
    {
        public string Message { get; set; } = string.Empty;

        public List<TechGridChatMessage> History { get; set; } = new();

        public TechGridChatGrid Grid { get; set; } = new();
    }

    public sealed class TechGridChatMessage
    {
        /// <summary>
        /// user / assistant
        /// </summary>
        public string Role { get; set; } = "user";

        public string Content { get; set; } = string.Empty;
    }

    public sealed class TechGridChatGrid
    {
        public string? Id { get; set; }

        public int Total { get; set; }

        public List<TechGridChatColumn> Columns { get; set; } = new();

        public TechGridChatState State { get; set; } = new();
    }

    public sealed class TechGridChatColumn
    {
        public string Field { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? DataType { get; set; }

        public bool Sortable { get; set; }

        public bool Filterable { get; set; }

        public bool Hidden { get; set; }
    }

    public sealed class TechGridChatState
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public List<TechGridSort> Sorts { get; set; } = new();

        public TechGridFilterGroup Filter { get; set; } = new();
    }

    /// <summary>
    /// Reply contract. <see cref="Reply"/> is shown in the chat; every action
    /// is validated and applied client-side against the current grid state.
    /// </summary>
    public sealed class TechGridChatResponse
    {
        public string Reply { get; set; } = string.Empty;

        public List<TechGridChatAction> Actions { get; set; } = new();
    }

    public sealed class TechGridChatAction
    {
        /// <summary>
        /// filter, clearFilters, sort, clearSorts, page, pageSize,
        /// hideColumns, showColumns, showAllColumns, select, highlight,
        /// clearHighlight, refresh
        /// </summary>
        public string Type { get; set; } = string.Empty;

        public string? Field { get; set; }

        public string? Operator { get; set; }

        public string? Value { get; set; }

        public string? Dir { get; set; }

        public int? Page { get; set; }

        public int? PageSize { get; set; }

        public List<string>? Fields { get; set; }
    }
}
