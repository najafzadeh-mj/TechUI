using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class CheckBoxBuilder : InputBuilderBase<CheckBoxBuilder>
    {
        private string? _label;
        private bool _switch;

        public CheckBoxBuilder(string name)
            : base(name, "form-check-input tech-input-control")
        {
        }

        public CheckBoxBuilder Label(string text)
        {
            _label = text;
            return this;
        }

        public CheckBoxBuilder Switch(bool enabled = true)
        {
            _switch = enabled;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper(
                "checkbox", "tech-choice", "tech-choice-checkbox");

            if (_switch)
            {
                wrapper.AddCssClass("tech-choice-switch");
            }

            if (IsDisabled)
            {
                wrapper.AddCssClass("tech-choice-disabled");
            }

            if (IsReadOnly)
            {
                wrapper.Attributes["data-readonly"] = "true";
            }

            var isChecked = IsChecked;

            var input = new TagBuilder("input");

            input.Attributes["type"] = "checkbox";
            input.Attributes["value"] = "true";

            ApplyControlAttributes(input, includeName: true, includeValue: false);

            input.AddCssClass("tech-choice-input");

            if (isChecked)
            {
                input.Attributes["checked"] = "checked";
            }

            wrapper.InnerHtml.AppendHtml(input);

            if (!string.IsNullOrEmpty(_label))
            {
                var label = new TagBuilder("label");

                label.Attributes["for"] = InputName;
                label.AddCssClass("tech-choice-label");

                label.InnerHtml.Append(_label);

                wrapper.InnerHtml.AppendHtml(label);
            }

            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = InputName;
            hidden.Attributes["value"] = IsDisabled
                ? (isChecked ? "true" : "false")
                : "false";

            wrapper.InnerHtml.AppendHtml(hidden);

            ApplyValidationData(wrapper);

            writer.Write(Write(wrapper, encoder));
        }

        private bool IsChecked =>
            string.Equals(InputValue, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(InputValue, "on", StringComparison.OrdinalIgnoreCase) ||
            InputValue == "1";
    }
}
