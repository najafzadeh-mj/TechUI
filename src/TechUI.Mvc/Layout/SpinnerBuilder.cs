using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Layout
{
    public sealed class SpinnerBuilder : IHtmlContent
    {
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _size = "md";
        private string _color = "primary";
        private string? _label;

        public SpinnerBuilder Size(string size)
        {
            if (!string.IsNullOrWhiteSpace(size))
            {
                _size = size.Trim().ToLowerInvariant();
            }

            return this;
        }

        public SpinnerBuilder Color(string color)
        {
            if (!string.IsNullOrWhiteSpace(color))
            {
                _color = color.Trim();
            }

            return this;
        }

        public SpinnerBuilder Label(string text)
        {
            _label = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
            return this;
        }

        public SpinnerBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public SpinnerBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("span");

            root.AddCssClass("tech-spinner");
            root.AddCssClass($"tech-spinner-{_size}");
            root.AddCssClass($"tech-spinner-{_color}");

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            root.Attributes["role"] = "status";

            foreach (var attribute in _attributes)
            {
                root.Attributes[attribute.Key] = attribute.Value;
            }

            var circle = new TagBuilder("span");

            circle.AddCssClass("tech-spinner-circle");

            root.InnerHtml.AppendHtml(circle);

            if (_label is not null)
            {
                var label = new TagBuilder("span");

                label.AddCssClass("tech-spinner-label");
                label.InnerHtml.Append(_label);

                root.InnerHtml.AppendHtml(label);
            }
            else
            {
                var accessible = new TagBuilder("span");

                accessible.AddCssClass("tech-sr-only");
                accessible.InnerHtml.Append("در حال بارگذاری");

                root.InnerHtml.AppendHtml(accessible);
            }

            root.WriteTo(writer, encoder);
        }
    }
}
