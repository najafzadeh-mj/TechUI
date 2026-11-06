using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class TextBoxBuilder : InputBuilderBase<TextBoxBuilder>
    {
        private string _type = "text";
        private bool _clearable;
        private string? _inputMode;
        private string? _autoComplete;

        public TextBoxBuilder(string name) : base(name)
        {
        }

        public TextBoxBuilder Type(string type)
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                _type = type.Trim().ToLowerInvariant();
            }

            return this;
        }

        public TextBoxBuilder Clearable(bool clearable = true)
        {
            _clearable = clearable;
            return this;
        }

        public TextBoxBuilder InputMode(string mode)
        {
            _inputMode = string.IsNullOrWhiteSpace(mode)
                ? null
                : mode.Trim().ToLowerInvariant();

            return this;
        }

        public TextBoxBuilder AutoComplete(string value)
        {
            _autoComplete = string.IsNullOrWhiteSpace(value) ? null : value;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("textbox", "tech-textbox");

            var field = new TagBuilder("div");
            field.AddCssClass("tech-input-field");

            var prefix = BuildAddon(PrefixIconValue, "prefix");

            if (prefix is not null)
            {
                field.InnerHtml.AppendHtml(prefix);
            }

            var input = new TagBuilder("input");

            input.Attributes["type"] = _type;

            if (!string.IsNullOrWhiteSpace(_type) &&
                !_type.Equals("text", StringComparison.OrdinalIgnoreCase) &&
                !_type.Equals("checkbox", StringComparison.OrdinalIgnoreCase) &&
                !_type.Equals("radio", StringComparison.OrdinalIgnoreCase))
            {
                input.Attributes["autocomplete"] = _autoComplete ?? "off";
            }
            else if (!string.IsNullOrWhiteSpace(_autoComplete))
            {
                input.Attributes["autocomplete"] = _autoComplete;
            }

            if (!string.IsNullOrWhiteSpace(_inputMode))
            {
                input.Attributes["inputmode"] = _inputMode;
            }

            ApplyControlAttributes(input, includeName: true, includeValue: true);

            ApplyValidationData(input);

            field.InnerHtml.AppendHtml(input);

            if (_clearable)
            {
                var clear = new TagBuilder("button");

                clear.Attributes["type"] = "button";
                clear.Attributes["tabindex"] = "-1";
                clear.Attributes["aria-label"] = "پاک کردن";
                clear.AddCssClass("tech-input-clear");

                var icon = new TagBuilder("i");
                icon.AddCssClass("bi bi-x-lg");

                clear.InnerHtml.AppendHtml(icon);

                field.InnerHtml.AppendHtml(clear);
            }

            var suffix = BuildAddon(SuffixIconValue, "suffix");

            if (suffix is not null)
            {
                field.InnerHtml.AppendHtml(suffix);
            }

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
