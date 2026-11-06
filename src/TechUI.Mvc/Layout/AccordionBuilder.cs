using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Layout
{
    public sealed class AccordionBuilder : IHtmlContent
    {
        private readonly string _id;
        private readonly List<AccordionItem> _items = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private bool _multiple;
        private bool _flush;
        private string? _changeHandler;

        public AccordionBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-accordion-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string AccordionId => _id;

        public AccordionBuilder Item(string title, string content)
        {
            return Item(title, new HtmlString(content ?? string.Empty));
        }

        public AccordionBuilder Item(string title, string content, bool expanded)
        {
            return Item(title, new HtmlString(content ?? string.Empty), expanded);
        }

        public AccordionBuilder Item(
            string title,
            IHtmlContent content,
            bool expanded = false)
        {
            _items.Add(new AccordionItem
            {
                Title = title ?? string.Empty,
                Content = content ?? HtmlString.Empty,
                IsExpanded = expanded
            });

            return this;
        }

        public AccordionBuilder Multiple(bool multiple = true)
        {
            _multiple = multiple;
            return this;
        }

        public AccordionBuilder Flush(bool flush = true)
        {
            _flush = flush;
            return this;
        }

        public AccordionBuilder Change(string handler)
        {
            _changeHandler = handler;
            return this;
        }

        public AccordionBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public AccordionBuilder Attribute(string name, string value)
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
            root.AddCssClass("tech-accordion");

            if (_flush)
            {
                root.AddCssClass("tech-accordion-flush");
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            root.Attributes["data-tech-component"] = "accordion";
            root.Attributes["data-multiple"] =
                _multiple.ToString().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(_changeHandler))
            {
                root.Attributes["data-event-change"] = _changeHandler;
            }

            foreach (var attribute in _attributes)
            {
                root.Attributes[attribute.Key] = attribute.Value;
            }

            for (var index = 0; index < _items.Count; index++)
            {
                var item = _items[index];
                var headerId = $"{_id}-header-{index}";
                var bodyId = $"{_id}-body-{index}";

                var itemElement = new TagBuilder("div");

                itemElement.AddCssClass("tech-accordion-item");

                if (item.IsExpanded)
                {
                    itemElement.AddCssClass("is-open");
                }

                var header = new TagBuilder("button");

                header.Attributes["type"] = "button";
                header.Attributes["id"] = headerId;
                header.Attributes["aria-controls"] = bodyId;
                header.Attributes["aria-expanded"] =
                    item.IsExpanded ? "true" : "false";
                header.Attributes["data-accordion-index"] =
                    index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                header.AddCssClass("tech-accordion-header");

                var title = new TagBuilder("span");

                title.AddCssClass("tech-accordion-title");
                title.InnerHtml.Append(item.Title);

                var icon = new TagBuilder("i");

                icon.AddCssClass("bi bi-chevron-down");
                icon.AddCssClass("tech-accordion-icon");

                header.InnerHtml.AppendHtml(title);
                header.InnerHtml.AppendHtml(icon);

                var body = new TagBuilder("div");

                body.Attributes["id"] = bodyId;
                body.Attributes["role"] = "region";
                body.Attributes["aria-labelledby"] = headerId;
                body.AddCssClass("tech-accordion-body");

                var content = new TagBuilder("div");

                content.AddCssClass("tech-accordion-content");
                content.InnerHtml.AppendHtml(item.Content);

                body.InnerHtml.AppendHtml(content);

                itemElement.InnerHtml.AppendHtml(header);
                itemElement.InnerHtml.AppendHtml(body);

                root.InnerHtml.AppendHtml(itemElement);
            }

            root.WriteTo(writer, encoder);
        }

        private sealed class AccordionItem
        {
            public string Title { get; set; } = string.Empty;

            public IHtmlContent Content { get; set; } = HtmlString.Empty;

            public bool IsExpanded { get; set; }
        }
    }
}
