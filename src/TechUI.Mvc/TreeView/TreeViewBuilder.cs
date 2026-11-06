using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.TreeView
{
    /// <summary>
    /// Server-rendered hierarchical tree with optional checkboxes, selection
    /// and a Kendo-style client template.
    /// </summary>
    public sealed class TreeViewBuilder : IHtmlContent
    {
        private readonly string _id;
        private readonly List<TreeViewNode> _nodes = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _selection = "single";
        private bool _checkBoxes;
        private bool _expandAll;
        private string? _clientTemplate;
        private string? _selectHandler;
        private string? _checkHandler;

        public TreeViewBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-treeview-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string TreeViewId => _id;

        public TreeViewBuilder Items(Action<TreeViewNodeFactory> items)
        {
            if (items is not null)
            {
                items(new TreeViewNodeFactory(_nodes));
            }

            return this;
        }

        public TreeViewBuilder Selection(string? mode)
        {
            _selection = (mode ?? "single").Trim().ToLowerInvariant() switch
            {
                "none" => "none",
                "multiple" => "multiple",
                _ => "single"
            };

            return this;
        }

        public TreeViewBuilder CheckBoxes(bool checkBoxes = true)
        {
            _checkBoxes = checkBoxes;
            return this;
        }

        public TreeViewBuilder ExpandAll(bool expand = true)
        {
            _expandAll = expand;
            return this;
        }

        /// <summary>
        /// Kendo-style client template. Use <c>#: field #</c> for encoded
        /// output and <c>#= field #</c> for raw HTML.
        /// </summary>
        public TreeViewBuilder ClientTemplate(string? template)
        {
            _clientTemplate = string.IsNullOrWhiteSpace(template)
                ? null
                : template;

            return this;
        }

        public TreeViewBuilder ClientHtml(string? template)
        {
            return ClientTemplate(template);
        }

        public TreeViewBuilder Select(string handler)
        {
            _selectHandler = string.IsNullOrWhiteSpace(handler)
                ? null
                : handler.Trim();

            return this;
        }

        public TreeViewBuilder Check(string handler)
        {
            _checkHandler = string.IsNullOrWhiteSpace(handler)
                ? null
                : handler.Trim();

            return this;
        }

        public TreeViewBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public TreeViewBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var root = new TagBuilder("div");

            root.Attributes["id"] = _id;
            root.AddCssClass("tech-treeview");
            root.Attributes["data-tech-component"] = "treeview";
            root.Attributes["data-selection"] = _selection;

            if (_checkBoxes)
            {
                root.Attributes["data-checkboxes"] = "true";
            }

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

            if (_checkHandler is not null)
            {
                root.Attributes["data-event-check"] = _checkHandler;
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            foreach (var attribute in _attributes)
            {
                root.Attributes[attribute.Key] = attribute.Value;
            }

            var list = new TagBuilder("ul");

            list.AddCssClass("tech-treeview-root");
            list.Attributes["role"] = "tree";

            if (_selection == "multiple")
            {
                list.Attributes["aria-multiselectable"] = "true";
            }

            foreach (var node in _nodes)
            {
                list.InnerHtml.AppendHtml(TreeViewNodeRenderer.BuildNode(
                    node,
                    1,
                    _checkBoxes,
                    _expandAll));
            }

            root.InnerHtml.AppendHtml(list);

            root.WriteTo(writer, encoder);
        }
    }
}
