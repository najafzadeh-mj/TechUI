using System.Text;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Helpers for endpoints that delegate the chat to a language model.
    /// The grid never talks to a model directly; the endpoint owns the AI
    /// client and returns a <see cref="TechGridChatResponse"/>.
    /// </summary>
    public static class TechGridChatTools
    {
        public static readonly string[] AllowedOperators =
        {
            "contains", "notcontains", "startswith", "endswith",
            "eq", "neq", "gt", "gte", "lt", "lte",
            "in", "notin", "isnull", "isnotnull"
        };

        public static readonly string[] AllowedActionTypes =
        {
            "filter", "clearFilters", "sort", "clearSorts", "page", "pageSize",
            "hideColumns", "showColumns", "showAllColumns",
            "select", "highlight", "clearHighlight", "refresh"
        };

        public static string BuildSystemPrompt(IEnumerable<TechGridChatColumn> columns, string? gridTitle = null)
        {
            return BuildSystemPrompt(
                new TechGridChatGrid { Columns = columns?.ToList() ?? new List<TechGridChatColumn>() },
                gridTitle);
        }

        /// <summary>
        /// Describes the grid and the response contract. Include it as the
        /// system message together with the user's conversation.
        /// </summary>
        public static string BuildSystemPrompt(TechGridChatGrid grid, string? gridTitle = null)
        {
            grid ??= new TechGridChatGrid();

            var builder = new StringBuilder();

            builder.AppendLine("You are the assistant embedded in a data grid.");
            builder.AppendLine("Answer in the language the user writes in.");
            builder.AppendLine("Never invent rows or values: you only see column metadata, the current page state and the total row count.");
            builder.AppendLine("Never output SQL or HTML. Reply with short, plain text.");
            builder.AppendLine();

            if (!string.IsNullOrWhiteSpace(gridTitle))
            {
                builder.AppendLine($"Grid title: {gridTitle}");
            }

            builder.AppendLine($"Total rows: {grid.Total}");
            builder.AppendLine($"Current page: {grid.State.Page} (page size {grid.State.PageSize})");

            builder.AppendLine();
            builder.AppendLine("Columns (use the field name exactly as written):");

            foreach (var column in grid.Columns)
            {
                var flags = new List<string>();

                if (column.Sortable) flags.Add("sortable");
                if (column.Filterable) flags.Add("filterable");
                if (column.Hidden) flags.Add("hidden");

                builder.AppendLine(
                    $"- field=\"{column.Field}\" title=\"{column.Title}\" type=\"{column.DataType ?? "string"}\"" +
                    (flags.Count > 0 ? $" ({string.Join(", ", flags)})" : string.Empty));
            }

            builder.AppendLine();
            builder.AppendLine("Respond with JSON only, shaped like:");
            builder.AppendLine("{\"reply\":\"<markdown-free answer>\",\"actions\":[<actions>]}");
            builder.AppendLine();
            builder.AppendLine("Supported actions:");
            builder.AppendLine("- {\"type\":\"filter\",\"field\":\"<field>\",\"operator\":\"<op>\",\"value\":\"<value>\"}");
            builder.AppendLine("- {\"type\":\"clearFilters\"}");
            builder.AppendLine("- {\"type\":\"sort\",\"field\":\"<field>\",\"dir\":\"asc|desc\"}");
            builder.AppendLine("- {\"type\":\"clearSorts\"}");
            builder.AppendLine("- {\"type\":\"page\",\"page\":<number>}");
            builder.AppendLine("- {\"type\":\"pageSize\",\"pageSize\":<number>}");
            builder.AppendLine("- {\"type\":\"hideColumns\",\"fields\":[\"<field>\"]}");
            builder.AppendLine("- {\"type\":\"showColumns\",\"fields\":[\"<field>\"]}");
            builder.AppendLine("- {\"type\":\"showAllColumns\"}");
            builder.AppendLine("- {\"type\":\"select\",\"field\":\"<field>\",\"value\":\"<value>\"}");
            builder.AppendLine("- {\"type\":\"highlight\",\"field\":\"<field>\",\"value\":\"<value>\"}");
            builder.AppendLine("- {\"type\":\"clearHighlight\"}");
            builder.AppendLine("- {\"type\":\"refresh\"}");
            builder.AppendLine();
            builder.AppendLine($"Allowed operators: {string.Join(", ", AllowedOperators)}.");
            builder.AppendLine("Only use fields and capabilities listed above. Emit no actions when the request is a question; answer it in \"reply\" instead.");

            return builder.ToString();
        }
    }
}
