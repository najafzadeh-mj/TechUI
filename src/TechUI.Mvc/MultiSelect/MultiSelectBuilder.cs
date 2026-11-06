using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;
using TechUI.Mvc.DropDownList;
using TechUI.Mvc.Infrastructure;
using TechUI.Mvc.Inputs;

namespace TechUI.Mvc.MultiSelect
{
    /// <summary>
    /// Multi-select list with chips, optional filtering and free tag entry.
    /// </summary>
    public sealed class MultiSelectBuilder : IHtmlContent
    {
        private readonly IHtmlHelper _html;
        private readonly DropdownDataSourceOptions _dataSource = new();
        private readonly DropdownEventOptions _events = new();
        private readonly List<(string Value, string Text)> _items = new();
        private readonly List<string> _values = new();
        private readonly Dictionary<string, string> _attributes = new();

        private string _name;
        private string? _placeholder;
        private bool _filterable = true;
        private bool _serverFiltering;
        private int _minLength = 1;
        private bool _allowCustom;
        private int? _maxItems;
        private bool _disabled;
        private string _textField = "text";
        private string _valueField = "value";

        internal MultiSelectBuilder(IHtmlHelper html, string name)
        {
            _html = html;
            _name = name;
        }

        public MultiSelectBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public MultiSelectBuilder Value(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _values.Add(value);
            }

            return this;
        }

        public MultiSelectBuilder Values(params string[] values)
        {
            _values.Clear();

            foreach (var value in values ?? Array.Empty<string>())
            {
                Value(value);
            }

            return this;
        }

        public MultiSelectBuilder Values(IEnumerable<string> values)
        {
            _values.Clear();

            foreach (var value in values ?? Enumerable.Empty<string>())
            {
                Value(value);
            }

            return this;
        }

        public MultiSelectBuilder Items(
            IEnumerable<KeyValuePair<string, string>> items)
        {
            _items.Clear();

            if (items is not null)
            {
                foreach (var item in items)
                {
                    _items.Add((item.Key, item.Value));
                }
            }

            return this;
        }

        public MultiSelectBuilder Items(Action<ChoiceItemsBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var builder = new ChoiceItemsBuilder();

            action(builder);

            _items.Clear();
            _items.AddRange(builder.Items);

            return this;
        }

        public MultiSelectBuilder Placeholder(string text)
        {
            _placeholder = text;
            return this;
        }

        public MultiSelectBuilder Filterable(bool enabled = true)
        {
            _filterable = enabled;
            return this;
        }

        public MultiSelectBuilder ServerFiltering(bool enabled = true)
        {
            _serverFiltering = enabled;
            return this;
        }

        public MultiSelectBuilder MinLength(int length)
        {
            _minLength = length > 0 ? length : 1;
            return this;
        }

        public MultiSelectBuilder AllowCustom(bool enabled = true)
        {
            _allowCustom = enabled;
            return this;
        }

        public MultiSelectBuilder MaxItems(int maxItems)
        {
            _maxItems = maxItems > 0 ? maxItems : null;
            return this;
        }

        public MultiSelectBuilder Disabled(bool disabled = true)
        {
            _disabled = disabled;
            return this;
        }

        public MultiSelectBuilder DataTextField(string field)
        {
            _textField = field;
            return this;
        }

        public MultiSelectBuilder DataValueField(string field)
        {
            _valueField = field;
            return this;
        }

        public MultiSelectBuilder DataSource(
            Action<DropdownDataSourceBuilder> action)
        {
            var builder = new DropdownDataSourceBuilder(_dataSource, _html);
            action(builder);
            return this;
        }

        public MultiSelectBuilder Events(Action<DropdownEventBuilder> action)
        {
            var builder = new DropdownEventBuilder(_events);
            action(builder);
            return this;
        }

        public MultiSelectBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public MultiSelectBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("div");

            root.AddCssClass("tech-multiselect");
            root.Attributes["data-tech-component"] = "multiselect";
            root.Attributes["data-name"] = _name;
            root.Attributes["data-filterable"] =
                _filterable.ToString().ToLowerInvariant();
            root.Attributes["data-server-filtering"] =
                _serverFiltering.ToString().ToLowerInvariant();
            root.Attributes["data-min-length"] =
                _minLength.ToString(CultureInfo.InvariantCulture);
            root.Attributes["data-allow-custom"] =
                _allowCustom.ToString().ToLowerInvariant();
            root.Attributes["data-text-field"] = _textField;
            root.Attributes["data-value-field"] = _valueField;

            if (!string.IsNullOrWhiteSpace(_placeholder))
            {
                root.Attributes["data-placeholder"] = _placeholder;
            }

            if (_maxItems.HasValue)
            {
                root.Attributes["data-max-items"] =
                    _maxItems.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (_disabled)
            {
                root.AddCssClass("tech-multiselect-disabled");
            }

            var method = _dataSource.Method == TechHttpMethod.Post
                ? "POST"
                : "GET";

            root.Attributes["data-method"] = method;

            if (!string.IsNullOrWhiteSpace(_dataSource.Url))
            {
                root.Attributes["data-url"] = _dataSource.Url;
            }

            if (!string.IsNullOrWhiteSpace(_dataSource.DataFunction))
            {
                root.Attributes["data-data-function"] =
                    _dataSource.DataFunction;
            }

            if (_dataSource.Method == TechHttpMethod.Post)
            {
                var token = TechAntiforgery.GetToken(_html);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    root.Attributes["data-antiforgery"] = token;
                }
            }

            if (!string.IsNullOrWhiteSpace(_events.Change))
            {
                root.Attributes["data-event-change"] = _events.Change;
            }

            if (!string.IsNullOrWhiteSpace(_events.DataBound))
            {
                root.Attributes["data-event-bound"] = _events.DataBound;
            }

            if (!string.IsNullOrWhiteSpace(_events.Open))
            {
                root.Attributes["data-event-open"] = _events.Open;
            }

            if (!string.IsNullOrWhiteSpace(_events.Close))
            {
                root.Attributes["data-event-close"] = _events.Close;
            }

            foreach (var attribute in _attributes)
            {
                if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    root.AddCssClass(attribute.Value);
                }
                else
                {
                    root.Attributes[attribute.Key] = attribute.Value;
                }
            }

            root.InnerHtml.AppendHtml(BuildControl());
            root.InnerHtml.AppendHtml(BuildPanel());
            root.InnerHtml.AppendHtml(BuildValues());

            root.WriteTo(writer, encoder);
        }

        private TagBuilder BuildControl()
        {
            var control = new TagBuilder("div");

            control.AddCssClass("tech-multiselect-control");

            var tags = new TagBuilder("div");

            tags.AddCssClass("tech-multiselect-tags");

            foreach (var value in _values)
            {
                tags.InnerHtml.AppendHtml(BuildChip(value, ResolveText(value)));
            }

            control.InnerHtml.AppendHtml(tags);

            var input = new TagBuilder("input");

            input.Attributes["type"] = "text";
            input.Attributes["autocomplete"] = "off";
            input.Attributes["aria-autocomplete"] = "list";
            input.Attributes["role"] = "combobox";
            input.Attributes["aria-expanded"] = "false";
            input.AddCssClass("tech-multiselect-input");

            if (!string.IsNullOrWhiteSpace(_placeholder))
            {
                input.Attributes["placeholder"] = _placeholder;
            }

            if (_disabled)
            {
                input.Attributes["disabled"] = "disabled";
            }

            control.InnerHtml.AppendHtml(input);

            if (!_disabled)
            {
                var arrow = new TagBuilder("span");
                arrow.AddCssClass("tech-multiselect-arrow");
                arrow.InnerHtml.Append("▾");
                control.InnerHtml.AppendHtml(arrow);
            }

            return control;
        }

        private TagBuilder BuildChip(string value, string text)
        {
            var chip = new TagBuilder("span");

            chip.AddCssClass("tech-multiselect-chip");
            chip.Attributes["data-value"] = value;

            var label = new TagBuilder("span");
            label.AddCssClass("tech-multiselect-chip-text");
            label.InnerHtml.Append(text);

            chip.InnerHtml.AppendHtml(label);

            if (!_disabled)
            {
                var remove = new TagBuilder("button");

                remove.Attributes["type"] = "button";
                remove.Attributes["tabindex"] = "-1";
                remove.Attributes["aria-label"] = "حذف";
                remove.AddCssClass("tech-multiselect-chip-remove");
                remove.InnerHtml.Append("×");

                chip.InnerHtml.AppendHtml(remove);
            }

            return chip;
        }

        private TagBuilder BuildPanel()
        {
            var panel = new TagBuilder("div");

            panel.AddCssClass("tech-multiselect-panel");
            panel.Attributes["role"] = "listbox";
            panel.Attributes["aria-multiselectable"] = "true";

            var options = new TagBuilder("div");

            options.AddCssClass("tech-multiselect-options");

            foreach (var item in _items)
            {
                options.InnerHtml.AppendHtml(BuildOption(item.Value, item.Text));
            }

            panel.InnerHtml.AppendHtml(options);

            var empty = new TagBuilder("div");

            empty.AddCssClass("tech-multiselect-empty");
            empty.InnerHtml.Append("موردی یافت نشد");

            if (_items.Count > 0)
            {
                empty.Attributes["hidden"] = "hidden";
            }

            panel.InnerHtml.AppendHtml(empty);

            return panel;
        }

        private TagBuilder BuildOption(string value, string text)
        {
            var option = new TagBuilder("button");

            option.Attributes["type"] = "button";
            option.Attributes["role"] = "option";
            option.Attributes["data-value"] = value;
            option.Attributes["aria-selected"] =
                _values.Contains(value) ? "true" : "false";
            option.AddCssClass("tech-multiselect-option");

            if (_values.Contains(value))
            {
                option.AddCssClass("is-selected");
            }

            var check = new TagBuilder("i");

            check.AddCssClass("bi bi-check2 tech-multiselect-check");

            var label = new TagBuilder("span");

            label.AddCssClass("tech-multiselect-option-text");
            label.InnerHtml.Append(text);

            option.InnerHtml.AppendHtml(check);
            option.InnerHtml.AppendHtml(label);

            return option;
        }

        private TagBuilder BuildValues()
        {
            var values = new TagBuilder("div");

            values.AddCssClass("tech-multiselect-values");

            foreach (var value in _values)
            {
                var hidden = new TagBuilder("input");

                hidden.Attributes["type"] = "hidden";
                hidden.Attributes["name"] = _name;
                hidden.Attributes["value"] = value;

                values.InnerHtml.AppendHtml(hidden);
            }

            return values;
        }

        private string ResolveText(string value)
        {
            var match = _items.FirstOrDefault(item => item.Value == value);

            return string.IsNullOrEmpty(match.Text) ? value : match.Text;
        }
    }
}
