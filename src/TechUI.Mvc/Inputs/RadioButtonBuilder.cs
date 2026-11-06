using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class RadioButtonBuilder : InputBuilderBase<RadioButtonBuilder>
    {
        private readonly List<(string Value, string Text)> _items = new();

        private bool _inline;

        public RadioButtonBuilder(string name)
            : base(name, "form-check-input tech-input-control")
        {
        }

        public RadioButtonBuilder Items(Action<ChoiceItemsBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var builder = new ChoiceItemsBuilder();

            action(builder);

            _items.Clear();
            _items.AddRange(builder.Items);

            return this;
        }

        public RadioButtonBuilder Items(
            IEnumerable<KeyValuePair<string, string>> items)
        {
            _items.Clear();

            foreach (var item in items)
            {
                _items.Add((item.Key, item.Value));
            }

            return this;
        }

        public RadioButtonBuilder Inline(bool inline = true)
        {
            _inline = inline;
            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper(
                "radiogroup", "tech-choice", "tech-choice-radio");

            wrapper.Attributes["role"] = "radiogroup";
            wrapper.Attributes["data-inline"] =
                _inline.ToString().ToLowerInvariant();

            if (IsDisabled)
            {
                wrapper.AddCssClass("tech-choice-disabled");
            }

            if (IsReadOnly)
            {
                wrapper.Attributes["data-readonly"] = "true";
            }

            for (var index = 0; index < _items.Count; index++)
            {
                var item = _items[index];

                var itemId = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}_{1}",
                    InputName,
                    index);

                var label = new TagBuilder("label");

                label.AddCssClass("tech-choice-item");
                label.Attributes["for"] = itemId;

                var input = new TagBuilder("input");

                input.Attributes["type"] = "radio";
                input.Attributes["value"] = item.Value;

                ApplyControlAttributes(input, includeName: true, includeValue: false);

                input.Attributes["id"] = itemId;
                input.AddCssClass("tech-choice-input");

                if (string.Equals(InputValue, item.Value, StringComparison.Ordinal))
                {
                    input.Attributes["checked"] = "checked";
                }

                label.InnerHtml.AppendHtml(input);

                var text = new TagBuilder("span");

                text.AddCssClass("tech-choice-label");
                text.InnerHtml.Append(item.Text);

                label.InnerHtml.AppendHtml(text);

                wrapper.InnerHtml.AppendHtml(label);
            }

            ApplyValidationData(wrapper);

            writer.Write(Write(wrapper, encoder));
        }
    }
}
