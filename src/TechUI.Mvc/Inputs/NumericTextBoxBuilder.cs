using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class NumericTextBoxBuilder : InputBuilderBase<NumericTextBoxBuilder>
    {
        private decimal? _min;
        private decimal? _max;
        private decimal? _step;
        private int _decimals = 2;
        private bool _grouping = true;
        private string? _culture;
        private bool _spinButtons = true;

        public NumericTextBoxBuilder(string name) : base(name)
        {
        }

        public NumericTextBoxBuilder Min(decimal min)
        {
            _min = min;
            return this;
        }

        public NumericTextBoxBuilder Max(decimal max)
        {
            _max = max;
            return this;
        }

        public NumericTextBoxBuilder Step(decimal step)
        {
            _step = step > 0 ? step : null;
            return this;
        }

        public NumericTextBoxBuilder Decimals(int decimals)
        {
            _decimals = decimals < 0 ? 0 : decimals;
            return this;
        }

        public NumericTextBoxBuilder Grouping(bool grouping)
        {
            _grouping = grouping;
            return this;
        }

        public NumericTextBoxBuilder Culture(string culture)
        {
            _culture = string.IsNullOrWhiteSpace(culture) ? null : culture;
            return this;
        }

        public NumericTextBoxBuilder SpinButtons(bool show = true)
        {
            _spinButtons = show;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("numerictextbox", "tech-numeric");

            wrapper.Attributes["data-grouping"] =
                _grouping.ToString().ToLowerInvariant();

            wrapper.Attributes["data-decimals"] =
                _decimals.ToString(CultureInfo.InvariantCulture);

            if (!string.IsNullOrWhiteSpace(_culture))
            {
                wrapper.Attributes["data-culture"] = _culture;
            }

            if (_min.HasValue)
            {
                wrapper.Attributes["data-min"] =
                    _min.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (_max.HasValue)
            {
                wrapper.Attributes["data-max"] =
                    _max.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (_step.HasValue)
            {
                wrapper.Attributes["data-step"] =
                    _step.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(InputValue))
            {
                wrapper.Attributes["data-value"] = InputValue;
            }

            var field = new TagBuilder("div");
            field.AddCssClass("tech-input-field");

            var visible = new TagBuilder("input");

            visible.Attributes["type"] = "text";
            visible.Attributes["inputmode"] = "decimal";
            visible.Attributes["autocomplete"] = "off";

            ApplyControlAttributes(visible, includeName: false, includeValue: false);

            if (!visible.Attributes.ContainsKey("dir"))
            {
                visible.Attributes["dir"] = "ltr";
            }

            if (!string.IsNullOrEmpty(InputValue))
            {
                visible.Attributes["value"] = InputValue;
            }

            field.InnerHtml.AppendHtml(visible);

            if (_spinButtons)
            {
                var spin = new TagBuilder("span");
                spin.AddCssClass("tech-numeric-spin");

                spin.InnerHtml.AppendHtml(BuildSpinButton("up"));
                spin.InnerHtml.AppendHtml(BuildSpinButton("down"));

                field.InnerHtml.AppendHtml(spin);
            }

            wrapper.InnerHtml.AppendHtml(field);

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

            writer.Write(Write(wrapper, encoder));
        }

        private static TagBuilder BuildSpinButton(string direction)
        {
            var button = new TagBuilder("button");

            button.Attributes["type"] = "button";
            button.Attributes["tabindex"] = "-1";
            button.Attributes["data-spin"] = direction;
            button.Attributes["aria-label"] =
                direction == "up" ? "افزایش" : "کاهش";

            button.AddCssClass($"tech-numeric-spin-btn tech-numeric-{direction}");

            var icon = new TagBuilder("i");

            icon.AddCssClass(direction == "up"
                ? "bi bi-chevron-up"
                : "bi bi-chevron-down");

            button.InnerHtml.AppendHtml(icon);

            return button;
        }
    }
}
