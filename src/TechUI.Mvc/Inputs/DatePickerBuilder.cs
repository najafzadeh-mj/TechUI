using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class DatePickerBuilder : InputBuilderBase<DatePickerBuilder>
    {
        private string _format = "yyyy/MM/dd";
        private DateTime? _min;
        private DateTime? _max;
        private bool _todayButton = true;
        private bool _clearButton = true;
        private bool _openOnFocus = true;

        public DatePickerBuilder(string name) : base(name)
        {
        }

        public DatePickerBuilder Format(string format)
        {
            if (!string.IsNullOrWhiteSpace(format))
            {
                _format = format.Trim();
            }

            return this;
        }

        public DatePickerBuilder Min(DateTime min)
        {
            _min = min;
            return this;
        }

        public DatePickerBuilder Max(DateTime max)
        {
            _max = max;
            return this;
        }

        public DatePickerBuilder TodayButton(bool show = true)
        {
            _todayButton = show;
            return this;
        }

        public DatePickerBuilder ClearButton(bool show = true)
        {
            _clearButton = show;
            return this;
        }

        public DatePickerBuilder OpenOnFocus(bool open = true)
        {
            _openOnFocus = open;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("datepicker", "tech-datepicker");

            wrapper.Attributes["data-format"] = _format;
            wrapper.Attributes["data-today"] =
                _todayButton.ToString().ToLowerInvariant();
            wrapper.Attributes["data-clear"] =
                _clearButton.ToString().ToLowerInvariant();

            if (!_openOnFocus)
            {
                wrapper.Attributes["data-open-on-focus"] = "false";
            }

            if (_min.HasValue)
            {
                wrapper.Attributes["data-min"] = ToIso(_min.Value);
            }

            if (_max.HasValue)
            {
                wrapper.Attributes["data-max"] = ToIso(_max.Value);
            }

            var iso = ToIso(InputValue);

            if (iso is not null)
            {
                wrapper.Attributes["data-value"] = iso;
            }

            var field = new TagBuilder("div");
            field.AddCssClass("tech-input-field");

            var visible = new TagBuilder("input");

            visible.Attributes["type"] = "text";
            visible.Attributes["autocomplete"] = "off";

            ApplyControlAttributes(visible, includeName: false, includeValue: false);

            if (!visible.Attributes.ContainsKey("readonly"))
            {
                visible.Attributes["readonly"] = "readonly";
            }

            if (!visible.Attributes.ContainsKey("dir"))
            {
                visible.Attributes["dir"] = "ltr";
            }

            field.InnerHtml.AppendHtml(visible);

            var toggle = new TagBuilder("button");

            toggle.Attributes["type"] = "button";
            toggle.Attributes["tabindex"] = "-1";
            toggle.Attributes["aria-label"] = "انتخاب تاریخ";

            if (IsDisabled)
            {
                toggle.Attributes["disabled"] = "disabled";
            }

            toggle.AddCssClass("tech-datepicker-toggle");

            var icon = new TagBuilder("i");
            icon.AddCssClass("bi bi-calendar3");

            toggle.InnerHtml.AppendHtml(icon);

            field.InnerHtml.AppendHtml(toggle);

            wrapper.InnerHtml.AppendHtml(field);

            var panel = new TagBuilder("div");
            panel.AddCssClass("tech-datepicker-panel");
            panel.Attributes["role"] = "dialog";
            panel.Attributes["aria-label"] = "انتخاب تاریخ";

            wrapper.InnerHtml.AppendHtml(panel);

            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = InputName;
            hidden.Attributes["value"] = iso ?? string.Empty;

            ApplyValidationData(hidden);

            if (IsDisabled)
            {
                hidden.Attributes["disabled"] = "disabled";
            }

            wrapper.InnerHtml.AppendHtml(hidden);

            writer.Write(Write(wrapper, encoder));
        }

        private static string ToIso(DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string? ToIso(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateTime.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var exact))
            {
                return ToIso(exact);
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var invariant))
            {
                return ToIso(invariant);
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out var current))
            {
                return ToIso(current);
            }

            return null;
        }
    }
}
