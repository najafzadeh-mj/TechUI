using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class MaskedTextBoxBuilder : InputBuilderBase<MaskedTextBoxBuilder>
    {
        private string? _mask;
        private bool _unmaskedValue = true;

        public MaskedTextBoxBuilder(string name) : base(name)
        {
        }

        public MaskedTextBoxBuilder Mask(string mask)
        {
            _mask = string.IsNullOrWhiteSpace(mask) ? null : mask;
            return this;
        }

        public MaskedTextBoxBuilder UnmaskedValue(bool enabled = true)
        {
            _unmaskedValue = enabled;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("maskedtextbox", "tech-masked");

            if (!string.IsNullOrWhiteSpace(_mask))
            {
                wrapper.Attributes["data-mask"] = _mask;
            }

            wrapper.Attributes["data-unmasked"] =
                _unmaskedValue.ToString().ToLowerInvariant();

            var field = new TagBuilder("div");
            field.AddCssClass("tech-input-field");

            var visible = new TagBuilder("input");

            visible.Attributes["type"] = "text";
            visible.Attributes["inputmode"] = "numeric";
            visible.Attributes["autocomplete"] = "off";

            ApplyControlAttributes(
                visible,
                includeName: !_unmaskedValue,
                includeValue: false);

            if (!_unmaskedValue)
            {
                ApplyValidationData(visible);
            }

            if (!visible.Attributes.ContainsKey("dir"))
            {
                visible.Attributes["dir"] = "ltr";
            }

            if (!string.IsNullOrEmpty(InputValue))
            {
                visible.Attributes["value"] = InputValue;
            }

            field.InnerHtml.AppendHtml(visible);

            wrapper.InnerHtml.AppendHtml(field);

            if (_unmaskedValue)
            {
                var hidden = new TagBuilder("input");

                hidden.Attributes["type"] = "hidden";
                hidden.Attributes["name"] = InputName;
                hidden.Attributes["value"] = InputValue ?? string.Empty;

                ApplyValidationData(hidden);

                if (IsDisabled)
                {
                    hidden.Attributes["disabled"] = "disabled";
                }

                wrapper.InnerHtml.AppendHtml(hidden);
            }

            writer.Write(Write(wrapper, encoder));
        }
    }
}
