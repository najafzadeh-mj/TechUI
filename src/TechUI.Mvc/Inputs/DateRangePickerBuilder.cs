using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class DateRangePickerBuilder : InputBuilderBase<DateRangePickerBuilder>
    {
        private readonly string _endName;
        private string _format = "yyyy/MM/dd";
        private string _startPlaceholder = "تاریخ شروع";
        private string _endPlaceholder = "تاریخ پایان";
        private DateTime? _min;
        private DateTime? _max;
        private bool _todayButton = true;
        private bool _clearButton = true;
        private bool _openOnFocus = true;
        private string? _startValue;
        private string? _endValue;

        public DateRangePickerBuilder(string startName, string endName)
            : base(startName)
        {
            _endName = endName;
        }

        public string EndName => _endName;

        public DateRangePickerBuilder Format(string format)
        {
            if (!string.IsNullOrWhiteSpace(format))
            {
                _format = format.Trim();
            }

            return this;
        }

        public DateRangePickerBuilder Start(object? value)
        {
            _startValue = value is null
                ? null
                : Convert.ToString(value, CultureInfo.InvariantCulture);

            return this;
        }

        public DateRangePickerBuilder End(object? value)
        {
            _endValue = value is null
                ? null
                : Convert.ToString(value, CultureInfo.InvariantCulture);

            return this;
        }

        public DateRangePickerBuilder Values(object? start, object? end)
        {
            return Start(start).End(end);
        }

        public DateRangePickerBuilder Placeholders(
            string start,
            string end)
        {
            if (!string.IsNullOrWhiteSpace(start))
            {
                _startPlaceholder = start.Trim();
            }

            if (!string.IsNullOrWhiteSpace(end))
            {
                _endPlaceholder = end.Trim();
            }

            return this;
        }

        public DateRangePickerBuilder Min(DateTime min)
        {
            _min = min;
            return this;
        }

        public DateRangePickerBuilder Max(DateTime max)
        {
            _max = max;
            return this;
        }

        public DateRangePickerBuilder TodayButton(bool show = true)
        {
            _todayButton = show;
            return this;
        }

        public DateRangePickerBuilder ClearButton(bool show = true)
        {
            _clearButton = show;
            return this;
        }

        public DateRangePickerBuilder OpenOnFocus(bool open = true)
        {
            _openOnFocus = open;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("daterange", "tech-daterange");

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

            var startIso = ToIso(_startValue);
            var endIso = ToIso(_endValue);

            if (startIso is not null)
            {
                wrapper.Attributes["data-start"] = startIso;
            }

            if (endIso is not null)
            {
                wrapper.Attributes["data-end"] = endIso;
            }

            var field = new TagBuilder("div");
            field.AddCssClass("tech-input-field");
            field.AddCssClass("tech-daterange-field");

            var startInput = BuildVisibleInput(
                InputName,
                _startPlaceholder,
                "tech-daterange-start");

            field.InnerHtml.AppendHtml(startInput);

            var separator = new TagBuilder("span");
            separator.AddCssClass("tech-daterange-separator");
            separator.InnerHtml.Append("—");

            field.InnerHtml.AppendHtml(separator);

            var endInput = BuildVisibleInput(
                _endName,
                _endPlaceholder,
                "tech-daterange-end");

            field.InnerHtml.AppendHtml(endInput);

            var toggle = new TagBuilder("button");

            toggle.Attributes["type"] = "button";
            toggle.Attributes["tabindex"] = "-1";
            toggle.Attributes["aria-label"] = "انتخاب بازه تاریخ";
            toggle.AddCssClass("tech-daterange-toggle");

            if (IsDisabled)
            {
                toggle.Attributes["disabled"] = "disabled";
            }

            var icon = new TagBuilder("i");
            icon.AddCssClass("bi bi-calendar-range");

            toggle.InnerHtml.AppendHtml(icon);

            field.InnerHtml.AppendHtml(toggle);

            wrapper.InnerHtml.AppendHtml(field);

            var panel = new TagBuilder("div");
            panel.AddCssClass("tech-daterange-panel");
            panel.Attributes["role"] = "dialog";
            panel.Attributes["aria-label"] = "انتخاب بازه تاریخ";

            wrapper.InnerHtml.AppendHtml(panel);

            var startHidden = BuildHiddenInput(InputName, startIso);
            ApplyValidationData(startHidden);
            wrapper.InnerHtml.AppendHtml(startHidden);

            wrapper.InnerHtml.AppendHtml(BuildHiddenInput(_endName, endIso));

            writer.Write(Write(wrapper, encoder));
        }

        private TagBuilder BuildVisibleInput(
            string id,
            string placeholder,
            string cssClass)
        {
            var input = new TagBuilder("input");

            input.Attributes["type"] = "text";
            input.Attributes["autocomplete"] = "off";
            input.Attributes["id"] = id;
            input.Attributes["placeholder"] = placeholder;
            input.Attributes["dir"] = "ltr";

            ApplyControlAttributes(input, includeName: false, includeValue: false);

            input.Attributes["id"] = id;
            input.AddCssClass(cssClass);

            if (!input.Attributes.ContainsKey("readonly"))
            {
                input.Attributes["readonly"] = "readonly";
            }

            return input;
        }

        private TagBuilder BuildHiddenInput(string name, string? iso)
        {
            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = name;
            hidden.Attributes["value"] = iso ?? string.Empty;

            if (IsDisabled)
            {
                hidden.Attributes["disabled"] = "disabled";
            }

            return hidden;
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
