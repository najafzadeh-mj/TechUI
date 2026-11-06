using System.Text.Json;

namespace TechUI.Mvc.TreeView
{
    /// <summary>
    /// A single TreeView node and its fluent options.
    /// </summary>
    public sealed class TreeViewNode
    {
        private readonly Dictionary<string, object?> _data =
            new(StringComparer.OrdinalIgnoreCase);

        internal TreeViewNode(string id, string text)
        {
            Id = id;
            Text = text;
        }

        public string Id { get; }

        public string Text { get; private set; }

        internal string? NodeValue { get; private set; }

        internal string? NodeIcon { get; private set; }

        internal bool IsExpanded { get; private set; }

        internal bool IsDisabled { get; private set; }

        internal bool IsSelected { get; private set; }

        internal bool IsChecked { get; private set; }

        internal List<TreeViewNode> ChildNodes { get; } = new();

        public TreeViewNode Value(string value)
        {
            NodeValue = value;
            return this;
        }

        public TreeViewNode Icon(string icon)
        {
            NodeIcon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
            return this;
        }

        public TreeViewNode Expanded(bool expanded = true)
        {
            IsExpanded = expanded;
            return this;
        }

        public TreeViewNode Disabled(bool disabled = true)
        {
            IsDisabled = disabled;
            return this;
        }

        public TreeViewNode Selected(bool selected = true)
        {
            IsSelected = selected;
            return this;
        }

        public TreeViewNode Checked(bool isChecked = true)
        {
            IsChecked = isChecked;
            return this;
        }

        public TreeViewNode Data(string key, object? value)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                _data[key.Trim()] = value;
            }

            return this;
        }

        public TreeViewNode Children(Action<TreeViewNodeFactory> children)
        {
            if (children is null)
            {
                return this;
            }

            var factory = new TreeViewNodeFactory(ChildNodes);

            children(factory);

            return this;
        }

        internal string ToPayload()
        {
            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["id"] = Id,
                ["text"] = Text,
                ["value"] = NodeValue ?? Id,
                ["icon"] = NodeIcon ?? string.Empty,
                ["expanded"] = IsExpanded,
                ["disabled"] = IsDisabled,
                ["selected"] = IsSelected,
                ["checked"] = IsChecked
            };

            foreach (var item in _data)
            {
                if (!payload.ContainsKey(item.Key))
                {
                    payload[item.Key] = item.Value;
                }
            }

            return JsonSerializer.Serialize(payload);
        }
    }
}
