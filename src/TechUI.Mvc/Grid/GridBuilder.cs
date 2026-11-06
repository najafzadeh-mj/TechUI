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

        private int _pageSize = 10;
        private bool _pageSizeSelector;
        private int[] _pageSizeOptions = new[] { 10, 20, 50, 100 };

        private readonly GridDataSourceOptions _dataSource = new();
        private readonly GridEventOptions _events = new();
        private readonly List<GridToolbarItem> _toolbarItems = new();

        private bool _resizable;
        private int? _height;

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
            var container = new TagBuilder("div");

            container.Attributes["id"] = _name;
            container.AddCssClass("tech-grid");

            container.Attributes["data-tech-component"] = "grid";
            container.Attributes["data-pageable"] = _pageable.ToString().ToLowerInvariant();
            container.Attributes["data-sortable"] = _sortable.ToString().ToLowerInvariant();
            container.Attributes["data-filterable"] = _filterable.ToString().ToLowerInvariant();
            container.Attributes["data-selectable"] = _selectable.ToString().ToLowerInvariant();
            container.Attributes["data-resizable"] = _resizable.ToString().ToLowerInvariant();
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

            if (_dataSource.Method == TechHttpMethod.Post)
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

            if (!string.IsNullOrWhiteSpace(_events.Error))
            {
                container.Attributes["data-event-error"] = _events.Error;
            }

            var overlay = new TagBuilder("div");
            overlay.AddCssClass("tech-grid-loading");

            var spinner = new TagBuilder("div");
            spinner.AddCssClass("tech-grid-spinner");

            overlay.InnerHtml.AppendHtml(spinner);

            container.InnerHtml.AppendHtml(overlay);

            if (_toolbarItems.Count > 0)
            {
                var toolbar = new TagBuilder("div");
                toolbar.AddCssClass("tech-grid-toolbar d-flex align-items-center gap-2 p-2 border-bottom flex-wrap");

                foreach (var item in _toolbarItems)
                {
                    toolbar.InnerHtml.AppendHtml(BuildToolbarButton(item));
                }

                container.InnerHtml.AppendHtml(toolbar);
            }

            var table = new TagBuilder("table");
            table.AddCssClass("table table-striped table-hover mb-0");

            if (_resizable)
            {
                table.AddCssClass("tech-grid-table-fixed");
            }

            var thead = new TagBuilder("thead");
            var trHead = new TagBuilder("tr");

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

            tdEmpty.Attributes["colspan"] = Math.Max(_columns.Count, 1).ToString();
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
                isTemplate = column.IsTemplate,
                template = column.Template
            }));

            var toolbarJson = JsonSerializer.Serialize(_toolbarItems.Select(item => new
            {
                type = item.Type.ToString().ToLowerInvariant(),
                text = item.Text,
                handler = item.Handler,
                icon = item.Icon
            }));

            var nameJson = JsonSerializer.Serialize(_name);

            var script =
                "<script>" +
                "window.Tech = window.Tech || {};" +
                "Tech.GridOptions = Tech.GridOptions || {};" +
                $"Tech.GridOptions[{nameJson}] = {{ columns: {columnsJson}, toolbar: {toolbarJson} }};" +
                "</script>";

            writer.Write(stringWriter.ToString());
            writer.Write(script);
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
