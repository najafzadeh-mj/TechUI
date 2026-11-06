using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Wizard
{
    /// <summary>
    /// Multi-step form with optional step validation and linear navigation.
    /// </summary>
    public sealed class WizardBuilder : IHtmlContent
    {
        private readonly string _id;
        private readonly List<WizardStepItem> _steps = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly WizardEventOptions _events = new();

        private int _activeIndex;
        private bool _linear = true;
        private bool _vertical;
        private bool _showProgress = true;
        private bool _showControls = true;
        private bool _submitOnFinish;
        private string _prevText = "قبلی";
        private string _nextText = "بعدی";
        private string _finishText = "پایان";

        public WizardBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id)
                ? "tech-wizard-" + Guid.NewGuid().ToString("N")[..8]
                : id.Trim();
        }

        public string WizardId => _id;

        public WizardBuilder Step(
            string title,
            string content,
            string? description = null,
            string? icon = null)
        {
            return Step(
                title,
                new HtmlString(content ?? string.Empty),
                description,
                icon);
        }

        public WizardBuilder Step(
            string title,
            IHtmlContent content,
            string? description = null,
            string? icon = null)
        {
            _steps.Add(new WizardStepItem
            {
                Title = title ?? string.Empty,
                Description = description,
                Icon = icon,
                Content = content ?? HtmlString.Empty
            });

            return this;
        }

        public WizardBuilder Active(int index)
        {
            _activeIndex = index > 0 ? index : 0;
            return this;
        }

        public WizardBuilder Linear(bool linear = true)
        {
            _linear = linear;
            return this;
        }

        public WizardBuilder Vertical(bool vertical = true)
        {
            _vertical = vertical;
            return this;
        }

        public WizardBuilder Progress(bool show = true)
        {
            _showProgress = show;
            return this;
        }

        public WizardBuilder Controls(bool show = true)
        {
            _showControls = show;
            return this;
        }

        public WizardBuilder SubmitOnFinish(bool submit = true)
        {
            _submitOnFinish = submit;
            return this;
        }

        public WizardBuilder PrevText(string text)
        {
            _prevText = text ?? "قبلی";
            return this;
        }

        public WizardBuilder NextText(string text)
        {
            _nextText = text ?? "بعدی";
            return this;
        }

        public WizardBuilder FinishText(string text)
        {
            _finishText = text ?? "پایان";
            return this;
        }

        public WizardBuilder Events(Action<WizardEventBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var builder = new WizardEventBuilder(_events);
            action(builder);

            return this;
        }

        public WizardBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public WizardBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public WizardBuilder Attributes(
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
            var start = _steps.Count == 0
                ? 0
                : Math.Clamp(_activeIndex, 0, _steps.Count - 1);
            var last = _steps.Count - 1;

            var root = new TagBuilder("div");

            root.Attributes["id"] = _id;
            root.AddCssClass("tech-wizard");
            root.Attributes["data-tech-component"] = "wizard";
            root.Attributes["data-start"] =
                start.ToString(CultureInfo.InvariantCulture);
            root.Attributes["data-linear"] = Bool(_linear);
            root.Attributes["data-submit-on-finish"] = Bool(_submitOnFinish);

            if (_vertical)
            {
                root.AddCssClass("tech-wizard-vertical");
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            if (!string.IsNullOrWhiteSpace(_events.Change))
            {
                root.Attributes["data-event-change"] = _events.Change;
            }

            if (!string.IsNullOrWhiteSpace(_events.Complete))
            {
                root.Attributes["data-event-complete"] = _events.Complete;
            }

            if (!string.IsNullOrWhiteSpace(_events.Finish))
            {
                root.Attributes["data-event-finish"] = _events.Finish;
            }

            foreach (var attribute in _attributes)
            {
                if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    root.AddCssClass(attribute.Value);
                }
                else if (!root.Attributes.ContainsKey(attribute.Key))
                {
                    root.Attributes[attribute.Key] = attribute.Value;
                }
            }

            root.InnerHtml.AppendHtml(BuildSteps(start));
            root.InnerHtml.AppendHtml(BuildProgress(start));
            root.InnerHtml.AppendHtml(BuildPanels(start));

            if (_showControls)
            {
                root.InnerHtml.AppendHtml(BuildFooter(start, last));
            }

            root.WriteTo(writer, encoder);
        }

        private TagBuilder BuildSteps(int start)
        {
            var list = new TagBuilder("ol");

            list.AddCssClass("tech-wizard-steps");
            list.Attributes["aria-label"] = "مراحل";

            for (var index = 0; index < _steps.Count; index++)
            {
                var item = _steps[index];
                var active = index == start;
                var completed = index < start;
                var stepId = $"{_id}-step-{index}";
                var panelId = $"{_id}-panel-{index}";

                var li = new TagBuilder("li");

                li.AddCssClass("tech-wizard-step");
                li.Attributes["data-step-index"] =
                    index.ToString(CultureInfo.InvariantCulture);

                if (active)
                {
                    li.AddCssClass("is-active");
                }

                if (completed)
                {
                    li.AddCssClass("is-completed");
                }

                var button = new TagBuilder("button");

                button.Attributes["type"] = "button";
                button.Attributes["id"] = stepId;
                button.Attributes["aria-controls"] = panelId;
                button.AddCssClass("tech-wizard-step-btn");

                if (active)
                {
                    button.Attributes["aria-current"] = "step";
                }

                var marker = new TagBuilder("span");

                marker.AddCssClass("tech-wizard-marker");
                marker.Attributes["aria-hidden"] = "true";

                var number = new TagBuilder("span");

                number.AddCssClass("tech-wizard-number");
                number.InnerHtml.Append(
                    (index + 1).ToString(CultureInfo.InvariantCulture));

                marker.InnerHtml.AppendHtml(number);

                var check = new TagBuilder("i");

                check.AddCssClass("bi bi-check2 tech-wizard-check");
                check.Attributes["aria-hidden"] = "true";

                marker.InnerHtml.AppendHtml(check);

                var label = new TagBuilder("span");

                label.AddCssClass("tech-wizard-label");

                if (!string.IsNullOrWhiteSpace(item.Icon))
                {
                    var icon = new TagBuilder("span");

                    icon.AddCssClass("tech-wizard-icon");
                    icon.Attributes["aria-hidden"] = "true";

                    var glyph = new TagBuilder("i");

                    glyph.AddCssClass($"bi bi-{item.Icon}");
                    icon.InnerHtml.AppendHtml(glyph);
                    label.InnerHtml.AppendHtml(icon);
                }

                var title = new TagBuilder("span");

                title.AddCssClass("tech-wizard-title");
                title.InnerHtml.Append(item.Title);

                label.InnerHtml.AppendHtml(title);

                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    var description = new TagBuilder("span");

                    description.AddCssClass("tech-wizard-desc");
                    description.InnerHtml.Append(item.Description);

                    label.InnerHtml.AppendHtml(description);
                }

                var status = new TagBuilder("span");

                status.AddCssClass("tech-wizard-sr");
                status.InnerHtml.Append(completed
                    ? "تکمیل‌شده"
                    : active ? "مرحلهٔ جاری" : string.Empty);

                button.InnerHtml.AppendHtml(marker);
                button.InnerHtml.AppendHtml(label);
                button.InnerHtml.AppendHtml(status);

                li.InnerHtml.AppendHtml(button);
                list.InnerHtml.AppendHtml(li);
            }

            return list;
        }

        private TagBuilder BuildProgress(int start)
        {
            var progress = new TagBuilder("div");

            progress.AddCssClass("tech-wizard-progress");
            progress.Attributes["role"] = "progressbar";
            progress.Attributes["aria-label"] = "پیشرفت مراحل";
            progress.Attributes["aria-valuemin"] = "0";
            progress.Attributes["aria-valuemax"] = "100";
            progress.Attributes["aria-valuenow"] =
                Percent(start).ToString(CultureInfo.InvariantCulture);

            if (!_showProgress)
            {
                progress.Attributes["hidden"] = "hidden";
            }

            var bar = new TagBuilder("span");

            bar.AddCssClass("tech-wizard-progress-bar");
            bar.Attributes["style"] =
                $"width:{Percent(start).ToString(CultureInfo.InvariantCulture)}%";

            progress.InnerHtml.AppendHtml(bar);

            return progress;
        }

        private TagBuilder BuildPanels(int start)
        {
            var body = new TagBuilder("div");

            body.AddCssClass("tech-wizard-body");

            for (var index = 0; index < _steps.Count; index++)
            {
                var item = _steps[index];
                var panel = new TagBuilder("section");

                panel.Attributes["id"] = $"{_id}-panel-{index}";
                panel.Attributes["role"] = "group";
                panel.Attributes["aria-labelledby"] = $"{_id}-step-{index}";
                panel.Attributes["tabindex"] = "0";
                panel.AddCssClass("tech-wizard-panel");
                panel.Attributes["data-step-index"] =
                    index.ToString(CultureInfo.InvariantCulture);

                if (index == start)
                {
                    panel.AddCssClass("is-active");
                }
                else
                {
                    panel.Attributes["hidden"] = "hidden";
                }

                panel.InnerHtml.AppendHtml(item.Content);

                body.InnerHtml.AppendHtml(panel);
            }

            return body;
        }

        private TagBuilder BuildFooter(int start, int last)
        {
            var footer = new TagBuilder("div");

            footer.AddCssClass("tech-wizard-footer");

            var previous = new TagBuilder("button");

            previous.Attributes["type"] = "button";
            previous.AddCssClass("tech-wizard-prev");
            previous.InnerHtml.Append(_prevText);

            if (start == 0)
            {
                previous.Attributes["hidden"] = "hidden";
            }

            var spacer = new TagBuilder("span");

            spacer.AddCssClass("tech-wizard-spacer");

            var next = new TagBuilder("button");

            next.Attributes["type"] = "button";
            next.AddCssClass("tech-wizard-next");
            next.InnerHtml.Append(_nextText);

            if (start >= last)
            {
                next.Attributes["hidden"] = "hidden";
            }

            var finish = new TagBuilder("button");

            finish.Attributes["type"] = "button";
            finish.AddCssClass("tech-wizard-finish");
            finish.InnerHtml.Append(_finishText);

            if (start < last)
            {
                finish.Attributes["hidden"] = "hidden";
            }

            footer.InnerHtml.AppendHtml(previous);
            footer.InnerHtml.AppendHtml(spacer);
            footer.InnerHtml.AppendHtml(next);
            footer.InnerHtml.AppendHtml(finish);

            return footer;
        }

        private int Percent(int start)
        {
            if (_steps.Count == 0)
            {
                return 0;
            }

            return (int)Math.Round(start * 100d / _steps.Count);
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private sealed class WizardStepItem
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string? Icon { get; set; }
            public IHtmlContent Content { get; set; } = HtmlString.Empty;
        }
    }
}
