using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using System.Text.Json;
using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Minimal GridBuilder for TechUI.
    /// Compatible with .NET 8/9.
    /// </summary>
    public sealed class GridBuilder<TModel> : IHtmlContent
    {
        private readonly IHtmlHelper _html;
        private readonly List<GridColumn> _columns = new();

        private string _name = $"techGrid_{Guid.NewGuid():N}";
        private bool _pageable;
        private bool _sortable;
        private bool _filterable;
        private bool _selectable;
        private bool _multiSelect;

        private int _pageSize = 10;
        private bool _pageSizeSelector;
        private int[] _pageSizeOptions = new[] { 10, 20, 50, 100 };

        private readonly GridDataSourceOptions _dataSource = new();
        private readonly GridEventOptions _events = new();
        private readonly List<GridToolbarItem> _toolbarItems = new();

        private GridChatOptions? _chat;

        private bool _resizable;
        private bool _reorderable;
        private bool _groupPanel;
        private bool _infiniteScroll;
        private int? _height;
        private string? _ariaLabel;
        private readonly List<TechGridSort> _initialSorts = new();
        private readonly List<GridSummaryItem> _summaryItems = new();
        private readonly List<GridGroup> _groups = new();

        internal GridBuilder(IHtmlHelper html)
        {
            _html = html;
        }

        public GridBuilder<TModel> Height(int height)
        {
            _height = height;
            return this;
        }

        public GridBuilder<TModel> Name(string name)
        {
            _name = name;
            return this;
        }

        public GridBuilder<TModel> Pageable(bool enabled = true)
        {
            _pageable = enabled;
            return this;
        }

        public GridBuilder<TModel> Sortable(bool enabled = true)
        {
            _sortable = enabled;
            return this;
        }

        public GridBuilder<TModel> Filterable(bool enabled = true)
        {
            _filterable = enabled;
            return this;
        }

        public GridBuilder<TModel> Selectable(bool enabled = true)
        {
            _selectable = enabled;
            return this;
        }

        /// <summary>
        /// Replaces the pager with an infinite scroller: the next page is
        /// requested when the body reaches its end, and the rows are
        /// appended to the already loaded ones. Server paging is still used;
        /// <see cref="PageSize"/> is the size of each loaded chunk.
        /// </summary>
        public GridBuilder<TModel> InfiniteScroll(bool enabled = true)
        {
            _infiniteScroll = enabled;
            return this;
        }

        /// <summary>
        /// Adds a checkbox column so several rows can be selected at once.
        /// Multi-select implies <see cref="Selectable"/>.
        /// </summary>
        public GridBuilder<TModel> MultiSelect(bool enabled = true)
        {
            _multiSelect = enabled;

            if (enabled) _selectable = true;

            return this;
        }

        public GridBuilder<TModel> PageSize(int pageSize)
        {
            _pageSize = pageSize > 0 ? pageSize : 10;
            return this;
        }

        public GridBuilder<TModel> Resizable(bool enabled = true)
        {
            _resizable = enabled;
            return this;
        }

        /// <summary>
        /// Allows reordering columns by dragging the grip in the header.
        /// The order is persisted in localStorage.
        /// </summary>
        public GridBuilder<TModel> Reorderable(bool enabled = true)
        {
            _reorderable = enabled;
            return this;
        }

        /// <summary>
        /// Sets the initial (server-rendered) sort. Call it more than once
        /// for multi-column sorting.
        /// </summary>
        public GridBuilder<TModel> Sort(string field, string dir = "asc")
        {
            if (string.IsNullOrWhiteSpace(field)) return this;

            var normalized = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase)
                ? "desc"
                : "asc";

            _initialSorts.RemoveAll(item =>
                string.Equals(item.Field, field, StringComparison.OrdinalIgnoreCase));

            _initialSorts.Add(new TechGridSort { Field = field, Dir = normalized });

            return this;
        }

        /// <summary>
        /// Accessible name for the grid, used on the container and table.
        /// </summary>
        public GridBuilder<TModel> AriaLabel(string label)
        {
            _ariaLabel = label;
            return this;
        }

        /// <summary>
        /// Declares a summary footer. Aggregates are computed by the server
        /// over the filtered data (all pages).
        /// </summary>
        public GridBuilder<TModel> Summary(
            Action<GridSummaryBuilder<TModel>> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            action(new GridSummaryBuilder<TModel>(_summaryItems));

            return this;
        }

        /// <summary>
        /// Declares row grouping levels. Rows of the current page are grouped
        /// in the browser; call By multiple times for nested groups.
        /// </summary>
        public GridBuilder<TModel> Group(
            Action<GridGroupBuilder<TModel>> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            action(new GridGroupBuilder<TModel>(_groups));

            return this;
        }

        /// <summary>
        /// Shows a toolbar panel that lets the user change or clear the
        /// grouping at runtime. The user's choice is kept in localStorage.
        /// </summary>
        public GridBuilder<TModel> GroupPanel(bool enabled = true)
        {
            _groupPanel = enabled;
            return this;
        }

        public GridBuilder<TModel> DataSource(Action<GridDataSourceBuilder> action)
        {
            var builder = new GridDataSourceBuilder(_dataSource, _html);
            action(builder);
            return this;
        }

        public GridBuilder<TModel> Events(Action<GridEventBuilder> action)
        {
            var builder = new GridEventBuilder(_events);
            action(builder);
            return this;
        }

        public GridBuilder<TModel> Toolbar(Action<GridToolbarBuilder> action)
        {
            var builder = new GridToolbarBuilder(_toolbarItems);
            action(builder);
            return this;
        }

        /// <summary>
        /// Enables the built-in AI chat panel. Commands are sent to
        /// <c>GridChatBuilder.Url</c>; TechUI ships no AI dependency.
        /// </summary>
        public GridBuilder<TModel> Chat(Action<GridChatBuilder> action)
        {
            var options = new GridChatOptions();

            action(new GridChatBuilder(options));

            _chat = options;

            return this;
        }

        public GridBuilder<TModel> PageSizeSelector(bool enabled = true)
        {
            _pageSizeSelector = enabled;
            return this;
        }

        public GridBuilder<TModel> PageSizes(params int[] sizes)
        {
            var validSizes = sizes?
                .Where(size => size > 0)
                .Distinct()
                .ToArray();

            if (validSizes is { Length: > 0 })
            {
                _pageSizeOptions = validSizes;
                _pageSizeSelector = true;
            }

            return this;
        }

        public GridBuilder<TModel> Columns(Action<GridColumnFactory<TModel>> action)
        {
            var factory = new GridColumnFactory<TModel>(_columns);
            action(factory);
            return this;
        }

        /// <summary>
        /// Renders HTML directly in Razor.
        /// </summary>
        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (_chat is not null && string.IsNullOrWhiteSpace(_chat.Url))
            {
                throw new InvalidOperationException(
                    "Grid chat requires an endpoint. Call .Chat(c => c.Url(...)).");
            }

            var container = new TagBuilder("div");

            container.Attributes["id"] = _name;
            container.AddCssClass("tech-grid");

            container.Attributes["data-tech-component"] = "grid";
            container.Attributes["data-pageable"] = _pageable.ToString().ToLowerInvariant();
            container.Attributes["data-sortable"] = _sortable.ToString().ToLowerInvariant();
            container.Attributes["data-filterable"] = _filterable.ToString().ToLowerInvariant();
            container.Attributes["data-selectable"] = _selectable.ToString().ToLowerInvariant();
            container.Attributes["data-multi-select"] = _multiSelect.ToString().ToLowerInvariant();
            container.Attributes["data-resizable"] = _resizable.ToString().ToLowerInvariant();
            container.Attributes["data-reorderable"] = _reorderable.ToString().ToLowerInvariant();
            container.Attributes["data-group-panel"] = _groupPanel.ToString().ToLowerInvariant();
            container.Attributes["data-infinite-scroll"] = _infiniteScroll.ToString().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(_ariaLabel))
            {
                container.Attributes["aria-label"] = _ariaLabel;
                container.Attributes["data-label"] = _ariaLabel;
            }
            container.Attributes["data-page-size"] = _pageSize.ToString();
            container.Attributes["data-page-size-selector"] = _pageSizeSelector.ToString().ToLowerInvariant();
            container.Attributes["data-page-sizes"] = string.Join(",", _pageSizeOptions);

            if (_height.HasValue)
            {
                container.Attributes["data-height"] = _height.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(_dataSource.Url))
            {
                container.Attributes["data-url"] = _dataSource.Url;
            }

            if (!string.IsNullOrWhiteSpace(_dataSource.DataFunction))
            {
                container.Attributes["data-data-function"] = _dataSource.DataFunction;
            }

            container.Attributes["data-method"] = _dataSource.Method == TechHttpMethod.Post
                ? "POST"
                : "GET";

            if (_dataSource.Method == TechHttpMethod.Post || _chat is not null)
            {
                var token = TechAntiforgery.GetToken(_html);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    container.Attributes["data-antiforgery"] = token;
                }
            }

            if (!string.IsNullOrWhiteSpace(_events.DataBinding))
            {
                container.Attributes["data-event-binding"] = _events.DataBinding;
            }

            if (!string.IsNullOrWhiteSpace(_events.DataBound))
            {
                container.Attributes["data-event-bound"] = _events.DataBound;
            }

            if (!string.IsNullOrWhiteSpace(_events.Change))
            {
                container.Attributes["data-event-change"] = _events.Change;
            }

            if (!string.IsNullOrWhiteSpace(_events.RowClick))
            {
                container.Attributes["data-event-row-click"] = _events.RowClick;
            }

            if (!string.IsNullOrWhiteSpace(_events.RowDoubleClick))
            {
                container.Attributes["data-event-row-double-click"] = _events.RowDoubleClick;
            }

            if (!string.IsNullOrWhiteSpace(_events.Error))
            {
                container.Attributes["data-event-error"] = _events.Error;
            }

            if (!string.IsNullOrWhiteSpace(_events.GroupToggle))
            {
                container.Attributes["data-event-group-toggle"] = _events.GroupToggle;
            }

            var overlay = new TagBuilder("div");
            overlay.AddCssClass("tech-grid-loading");

            var spinner = new TagBuilder("div");
            spinner.AddCssClass("tech-grid-spinner");

            overlay.InnerHtml.AppendHtml(spinner);

            container.InnerHtml.AppendHtml(overlay);

            if (_toolbarItems.Count > 0 || _chat is not null)
            {
                var toolbar = new TagBuilder("div");
                toolbar.AddCssClass("tech-grid-toolbar d-flex align-items-center gap-2 p-2 border-bottom flex-wrap");

                foreach (var item in _toolbarItems)
                {
                    toolbar.InnerHtml.AppendHtml(BuildToolbarButton(item));
                }

                if (_chat is not null)
                {
                    toolbar.InnerHtml.AppendHtml(BuildChatButton(_chat));
                }

                container.InnerHtml.AppendHtml(toolbar);
            }

            var table = new TagBuilder("table");
            table.AddCssClass("table table-striped table-hover mb-0");

            if (!string.IsNullOrWhiteSpace(_ariaLabel))
            {
                table.Attributes["aria-label"] = _ariaLabel;
            }

            if (_resizable)
            {
                table.AddCssClass("tech-grid-table-fixed");
            }

            var thead = new TagBuilder("thead");
            var trHead = new TagBuilder("tr");

            if (_multiSelect)
            {
                var thSelect = new TagBuilder("th");

                thSelect.Attributes["scope"] = "col";
                thSelect.AddCssClass("tech-grid-select-col");
                thSelect.InnerHtml.AppendHtml(
                    "<input type=\"checkbox\" class=\"tech-grid-select-all\" " +
                    "aria-label=\"انتخاب همه‌ی سطرها\">");

                trHead.InnerHtml.AppendHtml(thSelect);
            }

            foreach (var column in _columns)
            {
                var th = new TagBuilder("th");

                th.Attributes["scope"] = "col";

                if (column.Width.HasValue)
                {
                    th.Attributes["style"] = $"width:{column.Width.Value}px";
                }

                th.InnerHtml.Append(column.Title);

                trHead.InnerHtml.AppendHtml(th);
            }

            thead.InnerHtml.AppendHtml(trHead);

            var tbody = new TagBuilder("tbody");
            var trEmpty = new TagBuilder("tr");
            var tdEmpty = new TagBuilder("td");

            tdEmpty.Attributes["colspan"] =
                (Math.Max(_columns.Count, 1) + (_multiSelect ? 1 : 0)).ToString();
            tdEmpty.AddCssClass("text-center text-muted py-4");
            tdEmpty.InnerHtml.Append("داده‌ای بارگذاری نشده است.");

            trEmpty.InnerHtml.AppendHtml(tdEmpty);
            tbody.InnerHtml.AppendHtml(trEmpty);

            table.InnerHtml.AppendHtml(thead);
            table.InnerHtml.AppendHtml(tbody);

            var body = new TagBuilder("div");
            body.AddCssClass("tech-grid-body table-responsive");
            body.InnerHtml.AppendHtml(table);

            container.InnerHtml.AppendHtml(body);

            using var stringWriter = new StringWriter();
            container.WriteTo(stringWriter, encoder);

            var columnsJson = JsonSerializer.Serialize(_columns.Select(column => new
            {
                key = column.Key,
                field = column.Field,
                title = column.Title,
                dataType = column.DataType,
                width = column.Width,
                sortable = column.Sortable,
                filterable = column.Filterable,
                pinned = column.Pinned,
                isTemplate = column.IsTemplate,
                template = column.Template
            }));

            var toolbarJson = JsonSerializer.Serialize(_toolbarItems.Select(item => new
            {
                type = item.Type.ToString().ToLowerInvariant(),
                text = item.Text,
                handler = item.Handler,
                icon = item.Icon,
                fileName = item.FileName
            }));

            var nameJson = JsonSerializer.Serialize(_name);

            var sortsJson = JsonSerializer.Serialize(
                _initialSorts.Select(sort => new
                {
                    field = sort.Field,
                    dir = sort.Dir
                }));

            var summaryJson = JsonSerializer.Serialize(
                _summaryItems.Select(item => new
                {
                    field = item.Field,
                    type = item.Type.ToString().ToLowerInvariant(),
                    label = item.Label
                }));

            var groupsJson = JsonSerializer.Serialize(
                _groups.Select(group => new
                {
                    field = group.Field,
                    title = group.Title
                }));

            var chatJson = _chat is null
                ? "null"
                : JsonSerializer.Serialize(new
                {
                    url = _chat.Url,
                    title = _chat.Title,
                    placeholder = _chat.Placeholder,
                    welcome = _chat.Welcome,
                    buttonText = _chat.ButtonText,
                    icon = _chat.Icon,
                    maxLength = _chat.MaxLength,
                    suggestions = _chat.Suggestions
                });

            var script =
                "<script>" +
                "window.Tech = window.Tech || {};" +
                "Tech.GridOptions = Tech.GridOptions || {};" +
                $"Tech.GridOptions[{nameJson}] = {{ columns: {columnsJson}, toolbar: {toolbarJson}, chat: {chatJson}, sorts: {sortsJson}, summary: {summaryJson}, groups: {groupsJson} }};" +
                "</script>";

            writer.Write(stringWriter.ToString());
            writer.Write(script);
        }

        private static TagBuilder BuildChatButton(GridChatOptions chat)
        {
            var button = new TagBuilder("button");

            button.Attributes["type"] = "button";
            button.AddCssClass("btn btn-sm btn-primary tech-toolbar-btn tech-grid-chat-toggle ms-auto");
            button.Attributes["data-action"] = "chat";
            button.Attributes["aria-haspopup"] = "dialog";
            button.Attributes["aria-expanded"] = "false";

            var icon = new TagBuilder("i");
            icon.AddCssClass($"bi bi-{chat.Icon} me-1");
            button.InnerHtml.AppendHtml(icon);
            button.InnerHtml.Append(chat.ButtonText);

            return button;
        }

        private static TagBuilder BuildToolbarButton(GridToolbarItem item)
        {
            var button = new TagBuilder("button");

            button.Attributes["type"] = "button";
            button.AddCssClass("btn btn-sm btn-outline-primary tech-toolbar-btn");
            button.Attributes["data-action"] = item.Type.ToString().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(item.Handler))
            {
                button.Attributes["data-handler"] = item.Handler;
            }

            if (!string.IsNullOrWhiteSpace(item.FileName))
            {
                button.Attributes["data-file-name"] = item.FileName;
            }

            if (!string.IsNullOrWhiteSpace(item.Icon))
            {
                var icon = new TagBuilder("i");
                icon.AddCssClass($"bi bi-{item.Icon} me-1");
                button.InnerHtml.AppendHtml(icon);
            }

            button.InnerHtml.Append(item.Text);

            return button;
        }
    }
}
