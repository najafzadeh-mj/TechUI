using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Layout
{
    public sealed class TabsBuilder : IHtmlContent
    {
        private readonly string _id;
        private readonly List<TabItem> _items = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private bool _pills;
        private string? _changeHandler;

        public TabsBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-tabs-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string TabId => _id;

        public TabsBuilder Item(string title, string content)
        {
            return Item(title, new HtmlString(content ?? string.Empty));
        }

        public TabsBuilder Item(string title, string content, bool active)
        {
            return Item(title, new HtmlString(content ?? string.Empty), active);
        }

        public TabsBuilder Item(
            string title,
            IHtmlContent content,
            bool active = false)
        {
            var item = new TabItem
            {
                Title = title ?? string.Empty,
                Content = content ?? HtmlString.Empty
            };

            if (active && _items.Count > 0)
            {
                foreach (var existing in _items)
                {
                    existing.IsActive = false;
                }
            }

            item.IsActive = _items.Count == 0 || active;

            _items.Add(item);

            return this;
        }

        public TabsBuilder Active(int index)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].IsActive = i == index;
            }

            return this;
        }

        public TabsBuilder Pills(bool pills = true)
        {
            _pills = pills;
            return this;
        }

        public TabsBuilder Change(string handler)
        {
            _changeHandler = handler;
            return this;
        }

        public TabsBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public TabsBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("div");

            root.Attributes["id"] = _id;
            root.AddCssClass("tech-tabs");

            if (_pills)
            {
                root.AddCssClass("tech-tabs-pills");
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            root.Attributes["data-tech-component"] = "tabs";

            if (!string.IsNullOrWhiteSpace(_changeHandler))
            {
                root.Attributes["data-event-change"] = _changeHandler;
            }

            foreach (var attribute in _attributes)
            {
                root.Attributes[attribute.Key] = attribute.Value;
            }

            var nav = new TagBuilder("div");
            nav.AddCssClass("tech-tabs-nav");
            nav.Attributes["role"] = "tablist";

            var panels = new TagBuilder("div");
            panels.AddCssClass("tech-tabs-panels");

            for (var index = 0; index < _items.Count; index++)
            {
                var item = _items[index];
                var tabId = $"{_id}-tab-{index}";
                var panelId = $"{_id}-panel-{index}";

                var button = new TagBuilder("button");

                button.Attributes["type"] = "button";
                button.Attributes["id"] = tabId;
                button.Attributes["role"] = "tab";
                button.Attributes["aria-controls"] = panelId;
                button.Attributes["aria-selected"] =
                    item.IsActive ? "true" : "false";
                button.Attributes["tabindex"] =
                    item.IsActive ? "0" : "-1";
                button.Attributes["data-tab-index"] =
                    index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                button.AddCssClass("tech-tab");

                if (item.IsActive)
                {
                    button.AddCssClass("is-active");
                }

                button.InnerHtml.Append(item.Title);

                nav.InnerHtml.AppendHtml(button);

                var panel = new TagBuilder("div");

                panel.Attributes["id"] = panelId;
                panel.Attributes["role"] = "tabpanel";
                panel.Attributes["aria-labelledby"] = tabId;
                panel.Attributes["tabindex"] = "0";
                panel.AddCssClass("tech-tab-panel");

                if (item.IsActive)
                {
                    panel.AddCssClass("is-active");
                }

                panel.InnerHtml.AppendHtml(item.Content);

                panels.InnerHtml.AppendHtml(panel);
            }

            root.InnerHtml.AppendHtml(nav);
            root.InnerHtml.AppendHtml(panels);

            root.WriteTo(writer, encoder);
        }

        private sealed class TabItem
        {
            public string Title { get; set; } = string.Empty;

            public IHtmlContent Content { get; set; } = HtmlString.Empty;

            public bool IsActive { get; set; }
        }
    }
}
