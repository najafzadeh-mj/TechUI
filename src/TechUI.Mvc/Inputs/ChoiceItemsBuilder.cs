namespace TechUI.Mvc.Inputs
{
    public sealed class ChoiceItemsBuilder
    {
        internal List<(string Value, string Text)> Items { get; } = new();

        public ChoiceItemsBuilder Add(string value, string text)
        {
            Items.Add((value, text));
            return this;
        }

        public ChoiceItemsBuilder Add(string text)
        {
            return Add(text, text);
        }

        public ChoiceItemsBuilder AddRange(
            IEnumerable<KeyValuePair<string, string>> items)
        {
            foreach (var item in items)
            {
                Add(item.Key, item.Value);
            }

            return this;
        }
    }
}
