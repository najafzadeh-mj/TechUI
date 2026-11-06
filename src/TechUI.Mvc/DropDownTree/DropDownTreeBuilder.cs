using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;
using TechUI.Mvc.Inputs;
using TechUI.Mvc.TreeView;

namespace TechUI.Mvc.DropDownTree
{
    /// <summary>
    /// A dropdown control that picks values from a hierarchical tree.
    /// </summary>
    public sealed class DropDownTreeBuilder : IHtmlContent, IValidationBuilder
    {
        private readonly string _name;
        private string _id;
        private readonly List<TreeViewNode> _nodes = new();
        private readonly List<string> _values = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _validationAttributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _selection = "single";
        private bool? _checkBoxes;
        private bool _expandAll;
        private string _placeholder = "انتخاب کنید";
        private bool _clearable = true;
        private bool _disabled;
        private bool _readOnly;
        private bool _filterable;
        private bool _closeOnSelect;
        private bool _closeOnSelectSet;
        private int? _width;
        private string? _clientTemplate;
        private string? _selectHandler;
        private string? _changeHandler;
        private string? _checkHandler;
        private string? _openHandler;
        private string? _closeHandler;
        private string? _validationErrorText;

        internal DropDownTreeBuilder(string name, string? id = null)
        {
            _name = name;
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-droptree-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string DropDownTreeId => _id;

        public string FieldName => _name;

        public DropDownTreeBuilder Id(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                _id = id.Trim();
            }

            return this;
        }

        private bool Multiple => _selection == "multiple";

        private bool HasCheckBoxes => Multiple && (_checkBoxes ?? true);

        public DropDownTreeBuilder Value(string? value)
        {
            _values.Clear();

            if (!string.IsNullOrEmpty(value))
            {
                _values.Add(value);
            }

            return this;
        }

        public DropDownTreeBuilder Values(params string[] values)
        {
            return Values((IEnumerable<string>)(values ?? Array.Empty<string>()));
        }

        public DropDownTreeBuilder Values(IEnumerable<string> values)
        {
            _values.Clear();

            foreach (var value in values ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrEmpty(value) && !_values.Contains(value))
                {
                    _values.Add(value);
                }
            }

            return this;
        }

        public DropDownTreeBuilder Items(Action<TreeViewNodeFactory> items)
        {
            if (items is not null)
            {
                items(new TreeViewNodeFactory(_nodes));
            }

            return this;
        }

        public DropDownTreeBuilder Selection(string? mode)
        {
            _selection = (mode ?? "single").Trim().ToLowerInvariant() switch
            {
                "multiple" => "multiple",
                _ => "single"
            };

            return this;
        }

        public DropDownTreeBuilder CheckBoxes(bool enabled = true)
        {
            _checkBoxes = enabled;
            return this;
        }

        public DropDownTreeBuilder ExpandAll(bool expand = true)
        {
            _expandAll = expand;
            return this;
        }

        public DropDownTreeBuilder Placeholder(string text)
        {
            _placeholder = text ?? string.Empty;
            return this;
        }

        public DropDownTreeBuilder Clearable(bool enabled = true)
        {
            _clearable = enabled;
            return this;
        }

        public DropDownTreeBuilder Disabled(bool disabled = true)
        {
            _disabled = disabled;
            return this;
        }

        public DropDownTreeBuilder ReadOnly(bool readOnly = true)
        {
            _readOnly = readOnly;
            return this;
        }

        public DropDownTreeBuilder Filterable(bool enabled = true)
        {
            _filterable = enabled;
            return this;
        }

        public DropDownTreeBuilder Width(int width)
        {
            _width = width > 0 ? width : null;
            return this;
        }

        public DropDownTreeBuilder CloseOnSelect(bool close = true)
        {
            _closeOnSelect = close;
            _closeOnSelectSet = true;
            return this;
        }

        /// <summary>
        /// Kendo-style client template. Use <c>#: field #</c> for encoded
        /// output and <c>#= field #</c> for raw HTML.
        /// </summary>
        public DropDownTreeBuilder ClientTemplate(string? template)
        {
            _clientTemplate = string.IsNullOrWhiteSpace(template)
                ? null
                : template;

            return this;
        }

        public DropDownTreeBuilder ClientHtml(string? template)
        {
            return ClientTemplate(template);
        }

        public DropDownTreeBuilder Select(string handler)
        {
            _selectHandler = Clean(handler);
            return this;
        }

        public DropDownTreeBuilder Change(string handler)
        {
            _changeHandler = Clean(handler);
            return this;
        }

        public DropDownTreeBuilder Check(string handler)
        {
            _checkHandler = Clean(handler);
            return this;
        }

        public DropDownTreeBuilder Open(string handler)
        {
            _openHandler = Clean(handler);
            return this;
        }

        public DropDownTreeBuilder Close(string handler)
        {
            _closeHandler = Clean(handler);
            return this;
        }

        public DropDownTreeBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public DropDownTreeBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public DropDownTreeBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public DropDownTreeBuilder ValidationAttributes(
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

        public DropDownTreeBuilder ValidationError(string? message)
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
            var selectedValues = ResolveSelectedValues();

            ApplySelection(selectedValues);

            var hasError = !string.IsNullOrEmpty(_validationErrorText);

            var root = new TagBuilder("div");

            root.Attributes["id"] = _id;
            root.AddCssClass("tech-droptree");
            root.Attributes["data-tech-component"] = "droptree";
            root.Attributes["data-name"] = _name;
            root.Attributes["data-selection"] = _selection;
            root.Attributes["data-checkboxes"] =
                HasCheckBoxes.ToString().ToLowerInvariant();
            root.Attributes["data-placeholder"] = _placeholder;
            root.Attributes["data-clearable"] =
                (_clearable && !_disabled && !_readOnly)
                    .ToString().ToLowerInvariant();
            root.Attributes["data-filterable"] =
                _filterable.ToString().ToLowerInvariant();
            root.Attributes["data-close-on-select"] =
                EffectiveCloseOnSelect.ToString().ToLowerInvariant();

            if (_expandAll)
            {
                root.Attributes["data-expand-all"] = "true";
            }

            if (_clientTemplate is not null)
            {
                root.Attributes["data-client-template"] = _clientTemplate;
            }

            if (_selectHandler is not null)
            {
                root.Attributes["data-event-select"] = _selectHandler;
            }

            if (_changeHandler is not null)
            {
                root.Attributes["data-event-change"] = _changeHandler;
            }

            if (_checkHandler is not null)
            {
                root.Attributes["data-event-check"] = _checkHandler;
            }

            if (_openHandler is not null)
            {
                root.Attributes["data-event-open"] = _openHandler;
            }

            if (_closeHandler is not null)
            {
                root.Attributes["data-event-close"] = _closeHandler;
            }

            if (_disabled)
            {
                root.AddCssClass("tech-droptree-disabled");
            }

            if (_readOnly)
            {
                root.AddCssClass("tech-droptree-readonly");
            }

            if (hasError)
            {
                root.AddCssClass("tech-input-invalid");
            }

            if (_width.HasValue)
            {
                root.Attributes["style"] =
                    $"width:{_width.Value.ToString(CultureInfo.InvariantCulture)}px";
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
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

            root.InnerHtml.AppendHtml(
                BuildControl(selectedValues, hasError));
            root.InnerHtml.AppendHtml(BuildPanel());
            root.InnerHtml.AppendHtml(BuildValues(selectedValues));

            using var htmlWriter = new StringWriter();
            root.WriteTo(htmlWriter, encoder);

            var html = htmlWriter.ToString();

            if (_validationAttributes.Count > 0 || hasError)
            {
                var message = new TagBuilder("span");

                message.AddCssClass("tech-validation-message");
                message.Attributes["data-valmsg-for"] = _name;
                message.Attributes["data-valmsg-replace"] = "true";

                if (hasError)
                {
                    message.InnerHtml.Append(_validationErrorText!);
                }

                using var messageWriter = new StringWriter();
                message.WriteTo(messageWriter, encoder);
                html += messageWriter.ToString();
            }

            writer.Write(html);
        }

        private bool EffectiveCloseOnSelect =>
            _closeOnSelectSet ? _closeOnSelect : !Multiple;

        private TagBuilder BuildControl(
            IReadOnlyList<string> selectedValues,
            bool hasError)
        {
            var control = new TagBuilder("div");

            control.Attributes["id"] = _id + "-control";
            control.AddCssClass("tech-droptree-control");
            control.Attributes["role"] = "combobox";
            control.Attributes["aria-haspopup"] = "tree";
            control.Attributes["aria-expanded"] = "false";
            control.Attributes["aria-controls"] = _id + "-panel";
            control.Attributes["tabindex"] = _disabled ? "-1" : "0";

            if (hasError)
            {
                control.Attributes["aria-invalid"] = "true";
            }

            var values = new TagBuilder("div");

            values.AddCssClass("tech-droptree-values");

            if (selectedValues.Count == 0)
            {
                var placeholder = new TagBuilder("span");

                placeholder.AddCssClass("tech-droptree-placeholder");
                placeholder.InnerHtml.Append(_placeholder);
                values.InnerHtml.AppendHtml(placeholder);
            }
            else if (Multiple)
            {
                foreach (var value in selectedValues)
                {
                    values.InnerHtml.AppendHtml(BuildChip(
                        value,
                        ResolveText(value)));
                }
            }
            else
            {
                var text = new TagBuilder("span");

                text.AddCssClass("tech-droptree-text");
                text.InnerHtml.Append(ResolveText(selectedValues[0]));
                values.InnerHtml.AppendHtml(text);
            }

            control.InnerHtml.AppendHtml(values);

            if (_clearable && !_disabled && !_readOnly &&
                selectedValues.Count > 0)
            {
                var clear = new TagBuilder("button");

                clear.Attributes["type"] = "button";
                clear.Attributes["tabindex"] = "-1";
                clear.Attributes["aria-label"] = "پاک کردن";
                clear.AddCssClass("tech-droptree-clear");
                clear.InnerHtml.Append("×");

                control.InnerHtml.AppendHtml(clear);
            }

            if (!_disabled)
            {
                var arrow = new TagBuilder("span");

                arrow.AddCssClass("tech-droptree-arrow");
                arrow.InnerHtml.Append("▾");

                control.InnerHtml.AppendHtml(arrow);
            }

            return control;
        }

        private TagBuilder BuildChip(string value, string text)
        {
            var chip = new TagBuilder("span");

            chip.AddCssClass("tech-droptree-chip");
            chip.Attributes["data-value"] = value;

            var label = new TagBuilder("span");

            label.AddCssClass("tech-droptree-chip-text");
            label.InnerHtml.Append(text);
            chip.InnerHtml.AppendHtml(label);

            if (!_disabled && !_readOnly)
            {
                var remove = new TagBuilder("button");

                remove.Attributes["type"] = "button";
                remove.Attributes["tabindex"] = "-1";
                remove.Attributes["aria-label"] = "حذف";
                remove.AddCssClass("tech-droptree-chip-remove");
                remove.InnerHtml.Append("×");

                chip.InnerHtml.AppendHtml(remove);
            }

            return chip;
        }

        private TagBuilder BuildPanel()
        {
            var panel = new TagBuilder("div");

            panel.Attributes["id"] = _id + "-panel";
            panel.AddCssClass("tech-droptree-panel");

            if (_filterable)
            {
                var wrap = new TagBuilder("div");

                wrap.AddCssClass("tech-droptree-search-wrap");

                var search = new TagBuilder("input");

                search.Attributes["type"] = "text";
                search.Attributes["autocomplete"] = "off";
                search.Attributes["placeholder"] = "جستجو...";
                search.Attributes["aria-label"] = "جستجو";
                search.AddCssClass("tech-droptree-search");

                wrap.InnerHtml.AppendHtml(search);
                panel.InnerHtml.AppendHtml(wrap);
            }

            var tree = new TagBuilder("div");

            tree.AddCssClass("tech-treeview");
            tree.AddCssClass("tech-droptree-tree");

            var list = new TagBuilder("ul");

            list.AddCssClass("tech-treeview-root");
            list.Attributes["role"] = "tree";

            if (Multiple)
            {
                list.Attributes["aria-multiselectable"] = "true";
            }

            foreach (var node in _nodes)
            {
                list.InnerHtml.AppendHtml(TreeViewNodeRenderer.BuildNode(
                    node,
                    1,
                    HasCheckBoxes,
                    _expandAll));
            }

            tree.InnerHtml.AppendHtml(list);
            panel.InnerHtml.AppendHtml(tree);

            var empty = new TagBuilder("div");

            empty.AddCssClass("tech-droptree-empty");
            empty.InnerHtml.Append("موردی یافت نشد");

            if (_nodes.Count > 0)
            {
                empty.Attributes["hidden"] = "hidden";
            }

            panel.InnerHtml.AppendHtml(empty);

            return panel;
        }

        private TagBuilder BuildValues(IReadOnlyList<string> selectedValues)
        {
            var host = new TagBuilder("div");

            host.AddCssClass("tech-droptree-values-host");

            var first = BuildHidden(selectedValues.Count > 0
                ? selectedValues[0]
                : string.Empty);

            if (_validationAttributes.Count > 0)
            {
                first.Attributes["data-val"] = "true";

                foreach (var attribute in _validationAttributes)
                {
                    first.Attributes[attribute.Key] = attribute.Value;
                }
            }

            host.InnerHtml.AppendHtml(first);

            for (var index = 1; index < selectedValues.Count; index++)
            {
                host.InnerHtml.AppendHtml(
                    BuildHidden(selectedValues[index]));
            }

            return host;
        }

        private TagBuilder BuildHidden(string value)
        {
            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = _name;
            hidden.Attributes["value"] = value;

            return hidden;
        }

        private List<string> ResolveSelectedValues()
        {
            var values = new List<string>();

            void Add(string value)
            {
                if (!string.IsNullOrEmpty(value) && !values.Contains(value))
                {
                    values.Add(value);
                }
            }

            void Walk(IEnumerable<TreeViewNode> nodes)
            {
                foreach (var node in nodes)
                {
                    var value = NodeValue(node);

                    if (node.IsSelected ||
                        (HasCheckBoxes && node.IsChecked) ||
                        _values.Contains(value))
                    {
                        Add(value);
                    }

                    Walk(node.ChildNodes);
                }
            }

            Walk(_nodes);

            foreach (var value in _values)
            {
                Add(value);
            }

            if (!Multiple && values.Count > 1)
            {
                return new List<string> { values[0] };
            }

            return values;
        }

        private void ApplySelection(IReadOnlyList<string> values)
        {
            void Walk(IEnumerable<TreeViewNode> nodes)
            {
                foreach (var node in nodes)
                {
                    var selected = values.Contains(NodeValue(node));

                    node.Selected(selected);

                    if (HasCheckBoxes)
                    {
                        node.Checked(selected);
                    }

                    Walk(node.ChildNodes);
                }
            }

            Walk(_nodes);
        }

        private string ResolveText(string value)
        {
            var node = FindNodeByValue(_nodes, value);

            return node?.Text ?? value;
        }

        private static TreeViewNode? FindNodeByValue(
            IEnumerable<TreeViewNode> nodes,
            string value)
        {
            foreach (var node in nodes)
            {
                if (string.Equals(NodeValue(node), value, StringComparison.Ordinal))
                {
                    return node;
                }

                var match = FindNodeByValue(node.ChildNodes, value);

                if (match is not null)
                {
                    return match;
                }
            }

            return null;
        }

        private static string NodeValue(TreeViewNode node)
        {
            return node.NodeValue ?? node.Id;
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
