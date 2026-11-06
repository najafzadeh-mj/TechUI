using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using TechUI.Mvc.Infrastructure;
using TechUI.Mvc.Inputs;

namespace TechUI.Mvc.DropDownList
{
    public sealed class DropdownListBuilder : IHtmlContent, IValidationBuilder
    {
        private readonly IHtmlHelper _html;
        private readonly DropdownDataSourceOptions _dataSource = new();
        private readonly DropdownEventOptions _events = new();
        private readonly Dictionary<string, string> _attributes = new();
        private readonly Dictionary<string, string> _validationAttributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _name = $"techDropDown_{Guid.NewGuid():N}";
        private string? _value;
        private string? _optionLabel;
        private string? _validationErrorText;
        private int? _width;
        private string _textField = "text";
        private string _valueField = "value";
        private bool _filterable;
        private bool _serverFiltering;
        private int _minLength = 1;
        private FilterMode _filterMode;
        private string? _cascadeFrom;
        private string _cascadeParentField = "parentId";

        internal DropdownListBuilder(IHtmlHelper html)
        {
            _html = html;
        }

        public DropdownListBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public DropdownListBuilder Value(string? value)
        {
            _value = value;
            return this;
        }

        public DropdownListBuilder OptionLabel(string text)
        {
            _optionLabel = text;
            return this;
        }

        public DropdownListBuilder Width(int width)
        {
            _width = width;
            return this;
        }

        public DropdownListBuilder DataTextField(string field)
        {
            _textField = field;
            return this;
        }

        public DropdownListBuilder DataValueField(string field)
        {
            _valueField = field;
            return this;
        }

        public DropdownListBuilder DataSource(Action<DropdownDataSourceBuilder> action)
        {
            var builder = new DropdownDataSourceBuilder(_dataSource, _html);
            action(builder);
            return this;
        }

        public DropdownListBuilder Events(Action<DropdownEventBuilder> action)
        {
            var builder = new DropdownEventBuilder(_events);
            action(builder);
            return this;
        }

        public DropdownListBuilder Filterable(bool enabled = true)
        {
            _filterable = enabled;
            return this;
        }

        public DropdownListBuilder ServerFiltering(bool enabled = true)
        {
            _serverFiltering = enabled;
            return this;
        }

        public DropdownListBuilder MinLength(int length)
        {
            _minLength = length > 0 ? length : 1;
            return this;
        }

        public DropdownListBuilder FilterMode(FilterMode mode)
        {
            _filterMode = mode;
            return this;
        }

        public DropdownListBuilder CascadeFrom(string parentName)
        {
            _cascadeFrom = parentName;
            return this;
        }

        public DropdownListBuilder CascadeParentField(string field)
        {
            _cascadeParentField = field;
            return this;
        }

        public DropdownListBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public DropdownListBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public DropdownListBuilder ValidationAttributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (!string.IsNullOrWhiteSpace(attribute.Key))
                {
                    _validationAttributes[attribute.Key] = attribute.Value;
                }
            }

            return this;
        }

        public DropdownListBuilder ValidationError(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _validationErrorText = message;
            }

            return this;
        }

        void IValidationBuilder.ApplyValidation(
            IEnumerable<KeyValuePair<string, string>> attributes,
            string? errorMessage)
        {
            ValidationAttributes(attributes);
            ValidationError(errorMessage);
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var select = new TagBuilder("select");

            select.Attributes["id"] = _name;
            select.Attributes["name"] = _name;

            select.AddCssClass("tech-dropdown");
            select.AddCssClass("form-select");

            select.Attributes["data-tech-component"] = "dropdownlist";
            select.Attributes["data-text-field"] = _textField;
            select.Attributes["data-value-field"] = _valueField;
            select.Attributes["data-filterable"] = _filterable.ToString().ToLowerInvariant();
            select.Attributes["data-server-filtering"] = _serverFiltering.ToString().ToLowerInvariant();
            select.Attributes["data-min-length"] = _minLength.ToString();
            select.Attributes["data-filter-mode"] = _filterMode.ToString().ToLowerInvariant();

            var method = _dataSource.Method == TechHttpMethod.Post
                ? "POST"
                : "GET";

            select.Attributes["data-method"] = method;

            if (!string.IsNullOrWhiteSpace(_dataSource.Url))
            {
                select.Attributes["data-url"] = _dataSource.Url;
            }

            if (!string.IsNullOrWhiteSpace(_dataSource.DataFunction))
            {
                select.Attributes["data-data-function"] = _dataSource.DataFunction;
            }

            if (_dataSource.Method == TechHttpMethod.Post)
            {
                var token = TechAntiforgery.GetToken(_html);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    select.Attributes["data-antiforgery"] = token;
                }
            }

            if (_width.HasValue)
            {
                select.Attributes["style"] = $"width:{_width.Value}px";
            }

            if (!string.IsNullOrWhiteSpace(_events.Change))
            {
                select.Attributes["data-event-change"] = _events.Change;
            }

            if (!string.IsNullOrWhiteSpace(_events.DataBound))
            {
                select.Attributes["data-event-bound"] = _events.DataBound;
            }

            if (!string.IsNullOrWhiteSpace(_events.Open))
            {
                select.Attributes["data-event-open"] = _events.Open;
            }

            if (!string.IsNullOrWhiteSpace(_events.Close))
            {
                select.Attributes["data-event-close"] = _events.Close;
            }

            if (!string.IsNullOrWhiteSpace(_cascadeFrom))
            {
                select.Attributes["data-cascade-from"] = _cascadeFrom;
                select.Attributes["data-cascade-parent-field"] = _cascadeParentField;
                select.Attributes["disabled"] = "disabled";
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
                    select.AddCssClass(attribute.Value);
                }
                else
                {
                    select.Attributes[attribute.Key] = attribute.Value;
                }
            }

            if (_validationAttributes.Count > 0)
            {
                select.Attributes["data-val"] = "true";

                foreach (var attribute in _validationAttributes)
                {
                    select.Attributes[attribute.Key] = attribute.Value;
                }

                if (!string.IsNullOrEmpty(_validationErrorText))
                {
                    select.AddCssClass("is-invalid");
                    select.Attributes["aria-invalid"] = "true";
                }
            }

            if (!string.IsNullOrWhiteSpace(_optionLabel))
            {
                var option = new TagBuilder("option");
                option.Attributes["value"] = "";

                if (string.IsNullOrEmpty(_value))
                {
                    option.Attributes["selected"] = "selected";
                }

                option.InnerHtml.Append(_optionLabel);

                select.InnerHtml.AppendHtml(option);
            }

            if (!string.IsNullOrWhiteSpace(_value))
            {
                select.Attributes["data-value"] = _value;
            }

            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("tech-dropdown-wrapper");

            if (!string.IsNullOrEmpty(_validationErrorText))
            {
                wrapper.AddCssClass("tech-input-invalid");
            }

            if (_width.HasValue)
            {
                wrapper.Attributes["style"] = $"width:{_width.Value}px";
            }

            if (_filterable)
            {
                wrapper.AddCssClass("tech-dropdown-filterable");

                var display = new TagBuilder("div");
                display.AddCssClass("tech-dropdown-display");
                display.Attributes["role"] = "combobox";
                display.Attributes["aria-haspopup"] = "listbox";
                display.Attributes["aria-expanded"] = "false";
                display.Attributes["tabindex"] = "0";

                var text = new TagBuilder("span");
                text.AddCssClass("tech-dropdown-text");
                text.InnerHtml.Append(_optionLabel ?? "انتخاب کنید");

                var arrow = new TagBuilder("span");
                arrow.AddCssClass("tech-dropdown-arrow");
                arrow.InnerHtml.Append("▾");

                display.InnerHtml.AppendHtml(text);
                display.InnerHtml.AppendHtml(arrow);

                var panel = new TagBuilder("div");
                panel.AddCssClass("tech-dropdown-panel");

                var input = new TagBuilder("input");
                input.Attributes["type"] = "text";
                input.Attributes["placeholder"] = "جستجو...";
                input.Attributes["aria-label"] = "جستجو";
                input.Attributes["autocomplete"] = "off";
                input.AddCssClass("tech-dropdown-search");

                select.Attributes["size"] = "8";
                select.Attributes["role"] = "listbox";

                panel.InnerHtml.AppendHtml(input);
                panel.InnerHtml.AppendHtml(select);

                wrapper.InnerHtml.AppendHtml(display);
                wrapper.InnerHtml.AppendHtml(panel);
            }
            else
            {
                wrapper.InnerHtml.AppendHtml(select);
            }

            using var stringWriter = new StringWriter();
            wrapper.WriteTo(stringWriter, encoder);

            var html = stringWriter.ToString();

            if (_validationAttributes.Count > 0 ||
                !string.IsNullOrEmpty(_validationErrorText))
            {
                var message = new TagBuilder("span");

                message.AddCssClass("tech-validation-message");
                message.Attributes["data-valmsg-for"] = _name;
                message.Attributes["data-valmsg-replace"] = "true";

                if (!string.IsNullOrEmpty(_validationErrorText))
                {
                    message.InnerHtml.Append(_validationErrorText);
                }

                using var messageWriter = new StringWriter();
                message.WriteTo(messageWriter, encoder);
                html += messageWriter.ToString();
            }

            writer.Write(html);
        }
    }
}
