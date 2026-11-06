using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class TimePickerBuilder : InputBuilderBase<TimePickerBuilder>
    {
        private int _step = 1;
        private bool _showSeconds;
        private bool _use12Hour;
        private bool _nowButton = true;
        private bool _clearButton = true;
        private TimeSpan? _min;
        private TimeSpan? _max;

        public TimePickerBuilder(string name) : base(name)
        {
        }

        public TimePickerBuilder Step(int minutes)
        {
            _step = minutes switch
            {
                < 1 => 1,
                > 60 => 60,
                _ => minutes
            };

            return this;
        }

        public TimePickerBuilder ShowSeconds(bool show = true)
        {
            _showSeconds = show;
            return this;
        }

        public TimePickerBuilder Use12Hour(bool enabled = true)
        {
            _use12Hour = enabled;
            return this;
        }

        public TimePickerBuilder Min(TimeSpan min)
        {
            _min = min;
            return this;
        }

        public TimePickerBuilder Max(TimeSpan max)
        {
            _max = max;
            return this;
        }

        public TimePickerBuilder NowButton(bool show = true)
        {
            _nowButton = show;
            return this;
        }

        public TimePickerBuilder ClearButton(bool show = true)
        {
            _clearButton = show;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("timepicker", "tech-timepicker");

            wrapper.Attributes["data-step"] =
                _step.ToString(CultureInfo.InvariantCulture);

            wrapper.Attributes["data-seconds"] =
                _showSeconds.ToString().ToLowerInvariant();

            wrapper.Attributes["data-use12"] =
                _use12Hour.ToString().ToLowerInvariant();

            wrapper.Attributes["data-now"] =
                _nowButton.ToString().ToLowerInvariant();

            wrapper.Attributes["data-clear"] =
                _clearButton.ToString().ToLowerInvariant();

            if (_min.HasValue)
            {
                wrapper.Attributes["data-min"] = Format(_min.Value);
            }

            if (_max.HasValue)
            {
                wrapper.Attributes["data-max"] = Format(_max.Value);
            }

            var value = ToTime(InputValue);

            if (value.HasValue)
            {
                wrapper.Attributes["data-value"] = Format(value.Value);
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

            if (!visible.Attributes.ContainsKey("placeholder"))
            {
                visible.Attributes["placeholder"] =
                    _showSeconds ? "HH:mm:ss" : "HH:mm";
            }

            field.InnerHtml.AppendHtml(visible);

            var toggle = new TagBuilder("button");

            toggle.Attributes["type"] = "button";
            toggle.Attributes["tabindex"] = "-1";
            toggle.Attributes["aria-label"] = "انتخاب ساعت";

            if (IsDisabled)
            {
                toggle.Attributes["disabled"] = "disabled";
            }

            toggle.AddCssClass("tech-timepicker-toggle");

            var icon = new TagBuilder("i");
            icon.AddCssClass("bi bi-clock");

            toggle.InnerHtml.AppendHtml(icon);

            field.InnerHtml.AppendHtml(toggle);

            wrapper.InnerHtml.AppendHtml(field);

            var panel = new TagBuilder("div");
            panel.AddCssClass("tech-timepicker-panel");
            panel.Attributes["role"] = "dialog";
            panel.Attributes["aria-label"] = "انتخاب ساعت";

            wrapper.InnerHtml.AppendHtml(panel);

            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = InputName;
            hidden.Attributes["value"] = value.HasValue ? Format(value.Value) : string.Empty;

            ApplyValidationData(hidden);

            if (IsDisabled)
            {
                hidden.Attributes["disabled"] = "disabled";
            }

            wrapper.InnerHtml.AppendHtml(hidden);

            writer.Write(Write(wrapper, encoder));
        }

        private string Format(TimeSpan value)
        {
            return value.ToString(
                _showSeconds ? @"hh\:mm\:ss" : @"hh\:mm",
                CultureInfo.InvariantCulture);
        }

        private static TimeSpan? ToTime(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (TimeSpan.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    out var span) &&
                span >= TimeSpan.Zero &&
                span < TimeSpan.FromDays(1))
            {
                return span;
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var invariant))
            {
                return invariant.TimeOfDay;
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.CurrentCulture,
                    DateTimeStyles.None,
                    out var current))
            {
                return current.TimeOfDay;
            }

            return null;
        }
    }
}
