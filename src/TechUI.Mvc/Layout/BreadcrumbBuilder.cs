using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Layout
{
    /// <summary>
    /// Accessible page breadcrumb trail. Server rendered, no JavaScript.
    /// </summary>
    public sealed class BreadcrumbBuilder : IHtmlContent
    {
        private readonly List<BreadcrumbEntry> _items = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly string? _id;
        private string? _separator;
        private string _label = "مسیر صفحه";
        private bool _compact;

        public BreadcrumbBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        }

        public string? BreadcrumbId => _id;

        public BreadcrumbBuilder Item(string text, string? href = null, string? icon = null)
        {
            _items.Add(new BreadcrumbEntry
            {
                Text = text ?? string.Empty,
                Href = Text(href),
                Icon = NormalizeIcon(icon)
            });

            return this;
        }

        public BreadcrumbBuilder Home(
            string href = "/",
            string text = "خانه",
            string? icon = "house")
        {
            return Item(text, href, icon);
        }

        public BreadcrumbBuilder Separator(string? separator)
        {
            _separator = Text(separator);
            return this;
        }

        public BreadcrumbBuilder Label(string label)
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                _label = label.Trim();
            }

            return this;
        }

        public BreadcrumbBuilder Compact(bool compact = true)
        {
            _compact = compact;
            return this;
        }

        public BreadcrumbBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public BreadcrumbBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public BreadcrumbBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("nav");

            root.AddCssClass("tech-breadcrumb");
            root.Attributes["aria-label"] = _label;

            if (_compact)
            {
                root.AddCssClass("tech-breadcrumb-compact");
            }

            if (_id is not null)
            {
                root.Attributes["id"] = _id;
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            foreach (var attribute in _attributes)
            {
                if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    root.AddCssClass(attribute.Value);
                }
                else if (!root.Attributes.ContainsKey(attribute.Key))
                {
                    root.Attributes[attribute.Key] = attribute.Value;
                }
            }

            var list = new TagBuilder("ol");

            list.AddCssClass("tech-breadcrumb-list");

            for (var index = 0; index < _items.Count; index++)
            {
                var entry = _items[index];
                var isLast = index == _items.Count - 1;

                var item = new TagBuilder("li");

                item.AddCssClass("tech-breadcrumb-item");

                if (_separator is not null)
                {
                    item.Attributes["data-separator"] = _separator;
                }

                var content = isLast ? new TagBuilder("span") : BuildLink(entry);

                if (isLast)
                {
                    content.AddCssClass("tech-breadcrumb-current");
                    content.Attributes["aria-current"] = "page";
                }
                else
                {
                    content.AddCssClass("tech-breadcrumb-link");
                }

                if (entry.Icon is not null)
                {
                    var icon = new TagBuilder("i");

                    icon.AddCssClass($"bi bi-{entry.Icon}");
                    icon.Attributes["aria-hidden"] = "true";

                    content.InnerHtml.AppendHtml(icon);
                }

                var text = new TagBuilder("span");

                text.AddCssClass("tech-breadcrumb-text");
                text.InnerHtml.Append(entry.Text);

                content.InnerHtml.AppendHtml(text);

                item.InnerHtml.AppendHtml(content);
                list.InnerHtml.AppendHtml(item);
            }

            root.InnerHtml.AppendHtml(list);
            root.WriteTo(writer, encoder);
        }

        private static TagBuilder BuildLink(BreadcrumbEntry entry)
        {
            if (entry.Href is null)
            {
                return new TagBuilder("span");
            }

            var link = new TagBuilder("a");

            link.Attributes["href"] = entry.Href;

            return link;
        }

        private static string? Text(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizeIcon(string? icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
            {
                return null;
            }

            var value = icon.Trim();

            if (value.StartsWith("bi-", StringComparison.OrdinalIgnoreCase))
            {
                value = value[3..];
            }

            return value;
        }

        private sealed class BreadcrumbEntry
        {
            public string Text { get; set; } = string.Empty;
            public string? Href { get; set; }
            public string? Icon { get; set; }
        }
    }
}
