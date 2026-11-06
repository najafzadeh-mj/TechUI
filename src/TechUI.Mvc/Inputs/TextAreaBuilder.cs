using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class TextAreaBuilder : InputBuilderBase<TextAreaBuilder>
    {
        private int _rows = 3;
        private bool _autoGrow;

        public TextAreaBuilder(string name) : base(name)
        {
        }

        public TextAreaBuilder Rows(int rows)
        {
            _rows = rows > 0 ? rows : 1;
            return this;
        }

        public TextAreaBuilder AutoGrow(bool autoGrow = true)
        {
            _autoGrow = autoGrow;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("textarea", "tech-textarea");

            if (_autoGrow)
            {
                wrapper.Attributes["data-auto-grow"] = "true";
            }

            var field = new TagBuilder("div");
            field.AddCssClass("tech-input-field");

            var area = new TagBuilder("textarea");

            area.Attributes["rows"] = _rows.ToString(CultureInfo.InvariantCulture);

            ApplyControlAttributes(area, includeName: true, includeValue: false);

            ApplyValidationData(area);

            if (!string.IsNullOrEmpty(InputValue))
            {
                area.InnerHtml.Append(InputValue);
            }

            field.InnerHtml.AppendHtml(area);

            wrapper.InnerHtml.AppendHtml(field);

            var counter = BuildCounter();

            if (counter is not null)
            {
                wrapper.InnerHtml.AppendHtml(counter);
            }

            writer.Write(Write(wrapper, encoder));
        }
    }
}
