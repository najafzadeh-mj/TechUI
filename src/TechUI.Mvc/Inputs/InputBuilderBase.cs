using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    /// <summary>
    /// Shared fluent base for TechUI form inputs.
    /// </summary>
    public abstract class InputBuilderBase<TBuilder> : IHtmlContent, IValidationBuilder
        where TBuilder : InputBuilderBase<TBuilder>
    {
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _dataAttributes =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> _validationAttributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _name;
        private string? _value;
        private string? _placeholder;
        private string _cssClass = "form-control tech-input-control";
        private string? _direction;
        private string? _changeHandler;
        private bool _disabled;
        private bool _readOnly;
        private bool _required;
        private int? _maxLength;
        private bool _showCounter;
        private string? _prefixIcon;
        private string? _suffixIcon;
        private bool _showValidationMessage;
        private string? _validationErrorText;

        protected InputBuilderBase(
            string name,
            string cssClass = "form-control tech-input-control")
        {
            _name = name;
            _cssClass = cssClass;
        }

        protected string InputName => _name;
        protected string? InputValue => _value;
        protected string? PlaceholderValue => _placeholder;
        protected bool IsDisabled => _disabled;
        protected bool IsReadOnly => _readOnly;
        protected bool IsRequired => _required;
        protected int? MaxLengthValue => _maxLength;
        protected bool CounterEnabled => _showCounter;
        protected string? PrefixIconValue => _prefixIcon;
        protected string? SuffixIconValue => _suffixIcon;
        protected string? DirectionValue => _direction;
        protected string? ChangeHandlerValue => _changeHandler;
        protected bool ValidationEnabled => _showValidationMessage;

        private TBuilder Self => (TBuilder)this;

        public TBuilder Name(string name)
        {
            _name = name;
            return Self;
        }

        public TBuilder Value(object? value)
        {
            _value = value is null
                ? null
                : Convert.ToString(value, CultureInfo.InvariantCulture);

            return Self;
        }

        public TBuilder Placeholder(string text)
        {
            _placeholder = text;
            return Self;
        }

        public TBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClass = $"{_cssClass} {cssClass}";
            }

            return Self;
        }

        public TBuilder Disabled(bool disabled = true)
        {
            _disabled = disabled;
            return Self;
        }

        public TBuilder ReadOnly(bool readOnly = true)
        {
            _readOnly = readOnly;
            return Self;
        }

        public TBuilder Required(bool required = true)
        {
            _required = required;
            return Self;
        }

        public TBuilder MaxLength(int length)
        {
            _maxLength = length > 0 ? length : null;
            return Self;
        }

        public TBuilder ShowCounter(bool show = true)
        {
            _showCounter = show;
            return Self;
        }

        public TBuilder PrefixIcon(string icon)
        {
            _prefixIcon = icon;
            return Self;
        }

        public TBuilder SuffixIcon(string icon)
        {
            _suffixIcon = icon;
            return Self;
        }

        public TBuilder Direction(string direction)
        {
            _direction = string.IsNullOrWhiteSpace(direction)
                ? null
                : direction.Trim().ToLowerInvariant();

            return Self;
        }

        public TBuilder Change(string handler)
        {
            _changeHandler = handler;
            return Self;
        }

        public TBuilder Attribute(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Self;
            }

            _attributes[name] = value;

            return Self;
        }

        public TBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return Self;
        }

        public TBuilder Data(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Self;
            }

            var key = name.StartsWith("data-", StringComparison.OrdinalIgnoreCase)
                ? name[5..]
                : name;

            _dataAttributes[key] = value;

            return Self;
        }

        public TBuilder ValidationAttributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (string.IsNullOrWhiteSpace(attribute.Key))
                {
                    continue;
                }

                _validationAttributes[attribute.Key] = attribute.Value;
            }

            if (_validationAttributes.Count > 0)
            {
                _showValidationMessage = true;
            }

            return Self;
        }

        public TBuilder ValidationError(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _validationErrorText = message;
                _showValidationMessage = true;
            }

            return Self;
        }

        public TBuilder ValidationMessage(bool show = true)
        {
            _showValidationMessage = show;
            return Self;
        }

        void IValidationBuilder.ApplyValidation(
            IEnumerable<KeyValuePair<string, string>> attributes,
            string? errorMessage)
        {
            ValidationAttributes(attributes);
            ValidationError(errorMessage);
        }

        protected void ApplyValidationData(TagBuilder control)
        {
            if (_validationAttributes.Count == 0)
            {
                return;
            }

            control.Attributes["data-val"] = "true";

            foreach (var attribute in _validationAttributes)
            {
                control.Attributes[attribute.Key] = attribute.Value;
            }

            if (!string.IsNullOrEmpty(_validationErrorText))
            {
                control.AddCssClass("is-invalid");
                control.Attributes["aria-invalid"] = "true";
            }
        }

        protected TagBuilder CreateWrapper(string component, params string[] cssClasses)
        {
            var wrapper = new TagBuilder("div");

            wrapper.AddCssClass("tech-input");

            foreach (var cssClass in cssClasses)
            {
                if (!string.IsNullOrWhiteSpace(cssClass))
                {
                    wrapper.AddCssClass(cssClass);
                }
            }

            wrapper.Attributes["data-tech-component"] = component;

            if (!string.IsNullOrWhiteSpace(_changeHandler))
            {
                wrapper.Attributes["data-event-change"] = _changeHandler;
            }

            return wrapper;
        }

        protected void ApplyControlAttributes(
            TagBuilder control,
            bool includeName,
            bool includeValue)
        {
            control.Attributes["id"] = _name;

            if (includeName)
            {
                control.Attributes["name"] = _name;
            }

            control.AddCssClass(_cssClass);

            if (!string.IsNullOrWhiteSpace(_placeholder))
            {
                control.Attributes["placeholder"] = _placeholder;
            }

            if (_maxLength.HasValue)
            {
                control.Attributes["maxlength"] =
                    _maxLength.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (_disabled)
            {
                control.Attributes["disabled"] = "disabled";
            }

            if (_readOnly)
            {
                control.Attributes["readonly"] = "readonly";
            }

            if (_required)
            {
                control.Attributes["required"] = "required";
            }

            if (!string.IsNullOrWhiteSpace(_direction))
            {
                control.Attributes["dir"] = _direction;
            }

            if (includeValue && !string.IsNullOrWhiteSpace(_value))
            {
                control.Attributes["value"] = _value;
            }

            foreach (var attribute in _attributes)
            {
                if (attribute.Key.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                    attribute.Key.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    control.AddCssClass(attribute.Value);
                }
                else
                {
                    control.Attributes[attribute.Key] = attribute.Value;
                }
            }

            foreach (var attribute in _dataAttributes)
            {
                control.Attributes[$"data-{attribute.Key}"] = attribute.Value;
            }
        }

        protected static TagBuilder? BuildAddon(string? icon, string position)
        {
            if (string.IsNullOrWhiteSpace(icon))
            {
                return null;
            }

            var addon = new TagBuilder("span");

            addon.AddCssClass($"tech-input-addon tech-input-{position}");

            var iconElement = new TagBuilder("i");
            iconElement.AddCssClass($"bi bi-{icon}");

            addon.InnerHtml.AppendHtml(iconElement);

            return addon;
        }

        protected TagBuilder? BuildCounter()
        {
            if (!_showCounter)
            {
                return null;
            }

            var counter = new TagBuilder("span");

            counter.AddCssClass("tech-input-counter");

            var max = _maxLength?.ToString(CultureInfo.InvariantCulture);

            counter.InnerHtml.Append(string.IsNullOrEmpty(max)
                ? "0"
                : $"0/{max}");

            return counter;
        }

        protected string Write(TagBuilder element, HtmlEncoder encoder)
        {
            if (!string.IsNullOrEmpty(_validationErrorText))
            {
                element.AddCssClass("tech-input-invalid");
            }

            using var writer = new StringWriter();

            element.WriteTo(writer, encoder);

            if (_showValidationMessage)
            {
                var message = new TagBuilder("span");

                message.AddCssClass("tech-validation-message");
                message.Attributes["data-valmsg-for"] = _name;
                message.Attributes["data-valmsg-replace"] = "true";

                if (!string.IsNullOrEmpty(_validationErrorText))
                {
                    message.InnerHtml.Append(_validationErrorText);
                }

                message.WriteTo(writer, encoder);
            }

            return writer.ToString();
        }

        public abstract void WriteTo(TextWriter writer, HtmlEncoder encoder);
    }
}
