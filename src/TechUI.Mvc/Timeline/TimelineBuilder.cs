using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Timeline
{
    /// <summary>
    /// Vertical event timeline with tones, icons, badges and optional compact spacing.
    /// </summary>
    public sealed class TimelineBuilder : IHtmlContent
    {
        private static readonly HashSet<string> AllowedTones =
            new(StringComparer.OrdinalIgnoreCase)
            {
                TimelineTones.Primary,
                TimelineTones.Success,
                TimelineTones.Info,
                TimelineTones.Warning,
                TimelineTones.Danger,
                TimelineTones.Neutral
            };

        private readonly List<TimelineEntry> _items = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly string? _id;
        private bool _compact;

        public TimelineBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        }

        public string? TimelineId => _id;

        public TimelineBuilder Item(
            string title,
            string? time = null,
            string? description = null,
            string? icon = null,
            string? tone = null,
            string? badge = null)
        {
            _items.Add(new TimelineEntry
            {
                Title = title ?? string.Empty,
                Time = Text(time),
                Description = Text(description),
                Icon = NormalizeIcon(icon),
                Tone = NormalizeTone(tone),
                Badge = Text(badge)
            });

            return this;
        }

        public TimelineBuilder Item(
            string title,
            string? time,
            IHtmlContent content,
            string? icon = null,
            string? tone = null,
            string? badge = null)
        {
            _items.Add(new TimelineEntry
            {
                Title = title ?? string.Empty,
                Time = Text(time),
                Body = content,
                Icon = NormalizeIcon(icon),
                Tone = NormalizeTone(tone),
                Badge = Text(badge)
            });

            return this;
        }

        public TimelineBuilder Compact(bool compact = true)
        {
            _compact = compact;
            return this;
        }

        public TimelineBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public TimelineBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public TimelineBuilder Attributes(
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

            root.AddCssClass("tech-timeline");

            if (_compact)
            {
                root.AddCssClass("tech-timeline-compact");
            }

            if (_id is not null)
            {
                root.Attributes["id"] = _id;
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
                else if (!root.Attributes.ContainsKey(attribute.Key))
                {
                    root.Attributes[attribute.Key] = attribute.Value;
                }
            }

            var list = new TagBuilder("ol");

            list.AddCssClass("tech-timeline-list");

            foreach (var entry in _items)
            {
                list.InnerHtml.AppendHtml(BuildItem(entry));
            }

            root.InnerHtml.AppendHtml(list);
            root.WriteTo(writer, encoder);
        }

        private static TagBuilder BuildItem(TimelineEntry entry)
        {
            var item = new TagBuilder("li");

            item.AddCssClass("tech-timeline-item");

            if (entry.Tone is not null)
            {
                item.AddCssClass($"tech-timeline-{entry.Tone}");
            }

            var marker = new TagBuilder("span");

            marker.AddCssClass("tech-timeline-marker");
            marker.Attributes["aria-hidden"] = "true";

            if (entry.Icon is not null)
            {
                var icon = new TagBuilder("i");

                icon.AddCssClass($"bi bi-{entry.Icon}");
                marker.InnerHtml.AppendHtml(icon);
            }
            else
            {
                var dot = new TagBuilder("span");

                dot.AddCssClass("tech-timeline-dot");
                marker.InnerHtml.AppendHtml(dot);
            }

            var content = new TagBuilder("div");

            content.AddCssClass("tech-timeline-content");

            var head = new TagBuilder("div");

            head.AddCssClass("tech-timeline-head");

            var title = new TagBuilder("span");

            title.AddCssClass("tech-timeline-title");
            title.InnerHtml.Append(entry.Title);
            head.InnerHtml.AppendHtml(title);

            if (entry.Badge is not null)
            {
                var badge = new TagBuilder("span");

                badge.AddCssClass("tech-timeline-badge");
                badge.InnerHtml.Append(entry.Badge);
                head.InnerHtml.AppendHtml(badge);
            }

            if (entry.Time is not null)
            {
                var time = new TagBuilder("span");

                time.AddCssClass("tech-timeline-time");
                time.Attributes["dir"] = "auto";
                time.InnerHtml.Append(entry.Time);
                head.InnerHtml.AppendHtml(time);
            }

            content.InnerHtml.AppendHtml(head);

            if (entry.Body is not null)
            {
                var body = new TagBuilder("div");

                body.AddCssClass("tech-timeline-desc");
                body.InnerHtml.AppendHtml(entry.Body);
                content.InnerHtml.AppendHtml(body);
            }
            else if (entry.Description is not null)
            {
                var description = new TagBuilder("div");

                description.AddCssClass("tech-timeline-desc");
                description.InnerHtml.Append(entry.Description);
                content.InnerHtml.AppendHtml(description);
            }

            item.InnerHtml.AppendHtml(marker);
            item.InnerHtml.AppendHtml(content);

            return item;
        }

        private static string? Text(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizeIcon(string? icon)
        {
            if (string.IsNullOrWhiteSpace(icon))
            {
                return null;
            }

            var value = icon.Trim();

            if (value.StartsWith("bi-", StringComparison.OrdinalIgnoreCase))
            {
                value = value[3..];
            }

            return value;
        }

        private static string? NormalizeTone(string? tone)
        {
            if (string.IsNullOrWhiteSpace(tone))
            {
                return null;
            }

            var value = tone.Trim().ToLowerInvariant();

            return AllowedTones.Contains(value) ? value : null;
        }

        private sealed class TimelineEntry
        {
            public string Title { get; set; } = string.Empty;
            public string? Time { get; set; }
            public string? Description { get; set; }
            public IHtmlContent? Body { get; set; }
            public string? Icon { get; set; }
            public string? Tone { get; set; }
            public string? Badge { get; set; }
        }
    }
}
