using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace TechUI.Mvc.TreeView
{
    /// <summary>
    /// Renders the nested markup shared by TreeView and DropDownTree.
    /// </summary>
    internal static class TreeViewNodeRenderer
    {
        public static TagBuilder BuildNode(
            TreeViewNode node,
            int level,
            bool checkBoxes,
            bool expandAll)
        {
            var hasChildren = node.ChildNodes.Count > 0;
            var expanded = node.IsExpanded || expandAll;

            var item = new TagBuilder("li");

            item.AddCssClass("tech-tree-node");
            item.Attributes["role"] = "treeitem";
            item.Attributes["data-id"] = node.Id;
            item.Attributes["data-node"] = node.ToPayload();
            item.Attributes["aria-level"] =
                level.ToString(CultureInfo.InvariantCulture);
            item.Attributes["aria-selected"] =
                node.IsSelected ? "true" : "false";

            if (hasChildren)
            {
                item.Attributes["aria-expanded"] =
                    expanded ? "true" : "false";
            }

            if (expanded)
            {
                item.AddCssClass("is-expanded");
            }

            if (node.IsSelected)
            {
                item.AddCssClass("is-selected");
            }

            if (node.IsDisabled)
            {
                item.AddCssClass("is-disabled");
            }

            var row = new TagBuilder("div");

            row.AddCssClass("tech-tree-row");
            row.Attributes["tabindex"] = "-1";

            if (node.IsDisabled)
            {
                row.Attributes["aria-disabled"] = "true";
            }

            if (hasChildren)
            {
                var toggle = new TagBuilder("button");

                toggle.Attributes["type"] = "button";
                toggle.Attributes["tabindex"] = "-1";
                toggle.Attributes["aria-label"] =
                    expanded ? "بستن" : "باز کردن";
                toggle.AddCssClass("tech-tree-toggle");

                var chevron = new TagBuilder("i");

                chevron.AddCssClass("bi bi-chevron-down");

                toggle.InnerHtml.AppendHtml(chevron);
                row.InnerHtml.AppendHtml(toggle);
            }
            else
            {
                var spacer = new TagBuilder("span");

                spacer.AddCssClass("tech-tree-spacer");
                row.InnerHtml.AppendHtml(spacer);
            }

            if (checkBoxes)
            {
                var checkbox = new TagBuilder("input");

                checkbox.Attributes["type"] = "checkbox";
                checkbox.Attributes["tabindex"] = "-1";
                checkbox.Attributes["aria-label"] = node.Text;
                checkbox.AddCssClass(
                    "form-check-input tech-tree-checkbox");

                if (node.IsChecked)
                {
                    checkbox.Attributes["checked"] = "checked";
                }

                if (node.IsDisabled)
                {
                    checkbox.Attributes["disabled"] = "disabled";
                }

                row.InnerHtml.AppendHtml(checkbox);
            }

            if (!string.IsNullOrEmpty(node.NodeIcon))
            {
                var icon = new TagBuilder("i");

                icon.AddCssClass($"bi bi-{node.NodeIcon} tech-tree-icon");
                row.InnerHtml.AppendHtml(icon);
            }

            var label = new TagBuilder("span");

            label.AddCssClass("tech-tree-label");
            label.InnerHtml.Append(node.Text);
            row.InnerHtml.AppendHtml(label);

            item.InnerHtml.AppendHtml(row);

            if (hasChildren)
            {
                var children = new TagBuilder("ul");

                children.AddCssClass("tech-tree-children");
                children.Attributes["role"] = "group";

                foreach (var child in node.ChildNodes)
                {
                    children.InnerHtml.AppendHtml(
                        BuildNode(child, level + 1, checkBoxes, expandAll));
                }

                item.InnerHtml.AppendHtml(children);
            }

            return item;
        }
    }
}
