using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TechUI.Mvc.Chat
{
    /// <summary>
    /// Standalone conversation panel that can run as an AI assistant
    /// (single request/reply) or as a human support inbox (deliver +
    /// poll/push). Styling is driven by the shared <c>--tech-*</c> tokens,
    /// so dark mode and color presets apply automatically.
    /// </summary>
    public sealed class ChatBuilder : IHtmlContent
    {
        private readonly List<string> _suggestions = new();
        private readonly List<string> _cssClasses = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly string? _id;
        private TechChatMode _mode = TechChatMode.Assistant;
        private string? _url;
        private string? _pollUrl;
        private int _pollInterval = 4;
        private string? _title;
        private string? _subtitle;
        private string? _welcome;
        private string? _placeholder;
        private string? _icon;
        private int _maxLength = 1000;
        private int? _height;
        private bool _clearButton = true;

        public ChatBuilder(string? id = null)
        {
            _id = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        }

        public string? ChatId => _id;

        /// <summary>Assistant or support conversation style.</summary>
        public ChatBuilder Mode(TechChatMode mode)
        {
            _mode = mode;
            return this;
        }

        /// <summary>Endpoint that receives the visitor message
        /// (<see cref="TechChatRequest"/>).</summary>
        public ChatBuilder Url(string url)
        {
            _url = url;
            return this;
        }

        /// <summary>Optional GET endpoint polled for support replies
        /// (<see cref="TechChatResponse.Messages"/>).</summary>
        public ChatBuilder PollUrl(string pollUrl)
        {
            _pollUrl = pollUrl;
            return this;
        }

        /// <summary>Polling period in seconds (2–120).</summary>
        public ChatBuilder PollInterval(int seconds)
        {
            _pollInterval = Math.Clamp(seconds, 2, 120);
            return this;
        }

        public ChatBuilder Title(string title)
        {
            _title = title;
            return this;
        }

        public ChatBuilder Subtitle(string subtitle)
        {
            _subtitle = subtitle;
            return this;
        }

        public ChatBuilder Welcome(string welcome)
        {
            _welcome = welcome;
            return this;
        }

        public ChatBuilder Placeholder(string placeholder)
        {
            _placeholder = placeholder;
            return this;
        }

        /// <summary>Bootstrap Icon name without the <c>bi-</c> prefix.</summary>
        public ChatBuilder Icon(string icon)
        {
            _icon = string.IsNullOrWhiteSpace(icon)
                ? null
                : icon.Trim().StartsWith("bi-", StringComparison.OrdinalIgnoreCase)
                    ? icon.Trim()[3..]
                    : icon.Trim();

            return this;
        }

        public ChatBuilder MaxLength(int maxLength)
        {
            _maxLength = Math.Clamp(maxLength, 1, 4000);
            return this;
        }

        /// <summary>Panel height in pixels (280–1200).</summary>
        public ChatBuilder Height(int pixels)
        {
            _height = Math.Clamp(pixels, 280, 1200);
            return this;
        }

        public ChatBuilder Suggestions(params string[] suggestions)
        {
            _suggestions.Clear();

            if (suggestions is not null)
            {
                foreach (var suggestion in suggestions)
                {
                    if (!string.IsNullOrWhiteSpace(suggestion))
                    {
                        _suggestions.Add(suggestion.Trim());
                    }
                }
            }

            return this;
        }

        public ChatBuilder ClearButton(bool visible = true)
        {
            _clearButton = visible;
            return this;
        }

        public ChatBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClasses.Add(cssClass.Trim());
            }

            return this;
        }

        public ChatBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public ChatBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public ChatBuilder OnSend(string handler)
        {
            return Attribute("data-event-send", handler);
        }

        public ChatBuilder OnReceive(string handler)
        {
            return Attribute("data-event-receive", handler);
        }

        public ChatBuilder OnClear(string handler)
        {
            return Attribute("data-event-clear", handler);
        }

        public ChatBuilder OnError(string handler)
        {
            return Attribute("data-event-error", handler);
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (string.IsNullOrWhiteSpace(_url))
            {
                throw new InvalidOperationException(
                    "Tech Chat requires an endpoint. Call .Url(...).");
            }

            var assistant = _mode == TechChatMode.Assistant;

            var title = Text(_title) ?? (assistant ? "دستیار هوشمند" : "پشتیبانی آنلاین");
            var subtitle = Text(_subtitle) ?? (assistant
                ? "پاسخ فوری بر پایهٔ هوش مصنوعی"
                : "کارشناس پشتیبانی پاسخ می‌دهد");
            var welcome = Text(_welcome) ?? (assistant
                ? "سلام! چطور می‌توانم کمک کنم؟"
                : "سلام! پیام خود را بگذارید؛ به‌زودی پاسخ می‌دهیم.");
            var placeholder = Text(_placeholder) ?? (assistant
                ? "پیام خود را بنویسید…"
                : "پیام خود را برای پشتیبانی بنویسید…");
            var icon = _icon ?? (assistant ? "stars" : "headset");

            var root = new TagBuilder("div");

            root.AddCssClass("tech-chat");
            root.AddCssClass(assistant ? "tech-chat-assistant" : "tech-chat-support");
            root.Attributes["data-tech-component"] = "chat";
            root.Attributes["role"] = "region";
            root.Attributes["aria-label"] = title;

            if (_id is not null)
            {
                root.Attributes["id"] = _id;
            }

            foreach (var cssClass in _cssClasses)
            {
                root.AddCssClass(cssClass);
            }

            var options = JsonSerializer.Serialize(new
            {
                url = _url,
                mode = assistant ? "assistant" : "support",
                pollUrl = _pollUrl,
                pollInterval = _pollInterval,
                welcome,
                placeholder,
                maxLength = _maxLength,
                suggestions = _suggestions
            });

            root.Attributes["data-tech-chat"] = options;

            if (_height is int height)
            {
                root.Attributes["style"] = $"--tech-chat-height:{height}px";
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

            var header = new TagBuilder("div");
            header.AddCssClass("tech-chat-header");

            var agent = new TagBuilder("div");
            agent.AddCssClass("tech-chat-agent");

            var avatar = new TagBuilder("span");
            avatar.AddCssClass("tech-chat-avatar");
            avatar.Attributes["aria-hidden"] = "true";

            var avatarIcon = new TagBuilder("i");
            avatarIcon.AddCssClass($"bi bi-{icon}");
            avatar.InnerHtml.AppendHtml(avatarIcon);
            agent.InnerHtml.AppendHtml(avatar);

            var identity = new TagBuilder("div");
            identity.AddCssClass("tech-chat-identity");

            var titleTag = new TagBuilder("span");
            titleTag.AddCssClass("tech-chat-title");
            titleTag.InnerHtml.Append(title);
            identity.InnerHtml.AppendHtml(titleTag);

            var subtitleTag = new TagBuilder("span");
            subtitleTag.AddCssClass("tech-chat-subtitle");

            var status = new TagBuilder("span");
            status.AddCssClass("tech-chat-status");
            status.Attributes["aria-hidden"] = "true";
            subtitleTag.InnerHtml.AppendHtml(status);
            subtitleTag.InnerHtml.Append(subtitle);
            identity.InnerHtml.AppendHtml(subtitleTag);

            agent.InnerHtml.AppendHtml(identity);
            header.InnerHtml.AppendHtml(agent);

            if (_clearButton)
            {
                var tools = new TagBuilder("div");
                tools.AddCssClass("tech-chat-tools");

                var clear = new TagBuilder("button");
                clear.Attributes["type"] = "button";
                clear.AddCssClass("tech-chat-tool");
                clear.Attributes["data-chat-action"] = "clear";
                clear.Attributes["aria-label"] = "پاک‌کردن گفتگو";
                clear.Attributes["title"] = "پاک‌کردن گفتگو";

                var clearIcon = new TagBuilder("i");
                clearIcon.AddCssClass("bi bi-arrow-counterclockwise");
                clear.InnerHtml.AppendHtml(clearIcon);

                tools.InnerHtml.AppendHtml(clear);
                header.InnerHtml.AppendHtml(tools);
            }

            root.InnerHtml.AppendHtml(header);

            var log = new TagBuilder("div");
            log.AddCssClass("tech-chat-log");
            log.Attributes["role"] = "log";
            log.Attributes["aria-live"] = "polite";
            log.Attributes["aria-relevant"] = "additions";

            var welcomeTag = new TagBuilder("div");
            welcomeTag.AddCssClass("tech-chat-welcome");
            welcomeTag.InnerHtml.Append(welcome);
            log.InnerHtml.AppendHtml(welcomeTag);

            if (_suggestions.Count > 0)
            {
                var chips = new TagBuilder("div");
                chips.AddCssClass("tech-chat-suggestions");

                foreach (var suggestion in _suggestions)
                {
                    var chip = new TagBuilder("button");
                    chip.Attributes["type"] = "button";
                    chip.AddCssClass("tech-chat-chip");
                    chip.Attributes["data-text"] = suggestion;
                    chip.InnerHtml.Append(suggestion);
                    chips.InnerHtml.AppendHtml(chip);
                }

                log.InnerHtml.AppendHtml(chips);
            }

            root.InnerHtml.AppendHtml(log);

            var composer = new TagBuilder("form");
            composer.AddCssClass("tech-chat-composer");
            composer.Attributes["novalidate"] = "novalidate";

            var input = new TagBuilder("textarea");
            input.AddCssClass("tech-chat-input");
            input.Attributes["rows"] = "1";
            input.Attributes["maxlength"] = _maxLength.ToString();
            input.Attributes["placeholder"] = placeholder;
            input.Attributes["aria-label"] = "متن پیام";
            composer.InnerHtml.AppendHtml(input);

            var send = new TagBuilder("button");
            send.Attributes["type"] = "submit";
            send.AddCssClass("tech-chat-send");
            send.Attributes["aria-label"] = "ارسال";

            var sendIcon = new TagBuilder("i");
            sendIcon.AddCssClass("bi bi-send");
            send.InnerHtml.AppendHtml(sendIcon);

            composer.InnerHtml.AppendHtml(send);
            root.InnerHtml.AppendHtml(composer);

            root.WriteTo(writer, encoder);
        }

        private static string? Text(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}