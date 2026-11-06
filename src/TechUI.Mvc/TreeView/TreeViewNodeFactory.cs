namespace TechUI.Mvc.TreeView
{
    /// <summary>
    /// Collects the children of a TreeView node.
    /// </summary>
    public sealed class TreeViewNodeFactory
    {
        private readonly List<TreeViewNode> _nodes;

        internal TreeViewNodeFactory(List<TreeViewNode> nodes)
        {
            _nodes = nodes;
        }

        public TreeViewNodeFactory Add(
            string id,
            string text,
            Action<TreeViewNode>? configure = null)
        {
            var node = new TreeViewNode(
                string.IsNullOrWhiteSpace(id)
                    ? "node-" + Guid.NewGuid().ToString("N")[..8]
                    : id.Trim(),
                text ?? string.Empty);

            configure?.Invoke(node);

            _nodes.Add(node);

            return this;
        }
    }
}
