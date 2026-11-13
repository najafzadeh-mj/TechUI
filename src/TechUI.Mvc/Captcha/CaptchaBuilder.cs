using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Captcha
{
    /// <summary>
    /// Server-generated captcha image with a text answer and refresh button.
    /// </summary>
    public sealed class CaptchaBuilder : IHtmlContent
    {
        private readonly IHtmlHelper _html;
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _name;
        private CaptchaLanguage _language = CaptchaLanguage.Persian;
        private CaptchaKind _kind = CaptchaKind.Letters;
        private CaptchaMathOperation _operation = CaptchaMathOperation.Addition;
        private CaptchaNoiseLevel? _noise;
        private int _length;
        private string? _placeholder;
        private string _refreshUrl = "/techui/captcha";
        private string? _cssClass;
        private string? _error;
        private bool _disabled;
        private bool _required;

        internal CaptchaBuilder(IHtmlHelper html, string name)
        {
            _html = html;
            _name = name;
        }

        public CaptchaBuilder Name(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _name = name;
            }

            return this;
        }

        public CaptchaBuilder Language(CaptchaLanguage language)
        {
            _language = language;
            return this;
        }

        public CaptchaBuilder Kind(CaptchaKind kind)
        {
            _kind = kind;
            return this;
        }

        public CaptchaBuilder MathOperation(CaptchaMathOperation operation)
        {
            _operation = operation;
            return this;
        }

        public CaptchaBuilder Noise(CaptchaNoiseLevel noise)
        {
            _noise = noise;
            return this;
        }

        public CaptchaBuilder Length(int length)
        {
            _length = length;
            return this;
        }

        public CaptchaBuilder Placeholder(string text)
        {
            _placeholder = text;
            return this;
        }

        public CaptchaBuilder RefreshUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                _refreshUrl = url.Trim();
            }

            return this;
        }

        public CaptchaBuilder CssClass(string cssClass)
        {
            if (!string.IsNullOrWhiteSpace(cssClass))
            {
                _cssClass = string.IsNullOrWhiteSpace(_cssClass)
                    ? cssClass
                    : $"{_cssClass} {cssClass}";
            }

            return this;
        }

        public CaptchaBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public CaptchaBuilder Disabled(bool disabled = true)
        {
            _disabled = disabled;
            return this;
        }

        public CaptchaBuilder Required(bool required = true)
        {
            _required = required;
            return this;
        }

        public CaptchaBuilder ValidationError(string? message)
        {
            _error = message;
            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var httpContext = _html.ViewContext?.HttpContext
                ?? throw new InvalidOperationException(
                    "TechUI captcha must be rendered inside an HTTP request.");

            var service = httpContext.RequestServices
                .GetService<ITechCaptchaService>()
                ?? throw new InvalidOperationException(
                    "TechUI captcha requires services.AddTechUICaptcha().");

            var challenge = service.Create(new TechCaptchaRequest
            {
                Language = _language,
                Kind = _kind,
                Operation = _operation,
                Length = _length,
                Noise = _noise
            });

            var isEnglish = _language == CaptchaLanguage.English;
            var isNumeric = _kind != CaptchaKind.Letters;
            var tokenName = _name + "Token";

            var root = new TagBuilder("div");

            root.AddCssClass("tech-captcha");
            root.Attributes["data-tech-component"] = "captcha";
            root.Attributes["data-refresh-url"] = _refreshUrl;
            root.Attributes["data-language"] = isEnglish ? "en" : "fa";
            root.Attributes["data-kind"] = _kind switch
            {
                CaptchaKind.Digits => "digits",
                CaptchaKind.Math => "math",
                _ => "letters"
            };
            root.Attributes["data-operation"] = _operation switch
            {
                CaptchaMathOperation.Subtraction => "subtraction",
                CaptchaMathOperation.Multiplication => "multiplication",
                _ => "addition"
            };

            if (_kind != CaptchaKind.Math)
            {
                root.Attributes["data-length"] =
                    (_length > 0 ? Math.Clamp(_length, 3, 8) : 5)
                    .ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (_noise.HasValue)
            {
                root.Attributes["data-noise"] = _noise.Value switch
                {
                    CaptchaNoiseLevel.None => "none",
                    CaptchaNoiseLevel.Low => "low",
                    CaptchaNoiseLevel.High => "high",
                    _ => "medium"
                };
            }

            if (!string.IsNullOrWhiteSpace(_cssClass))
            {
                root.AddCssClass(_cssClass);
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

            var visual = new TagBuilder("div");

            visual.AddCssClass("tech-captcha-visual");

            var image = new TagBuilder("img");

            image.AddCssClass("tech-captcha-image");
            image.Attributes["src"] = challenge.ImageDataUri;
            image.Attributes["alt"] = isEnglish ? "Security code" : "کد امنیتی";
            image.Attributes["draggable"] = "false";

            visual.InnerHtml.AppendHtml(image);

            var refresh = new TagBuilder("button");

            refresh.Attributes["type"] = "button";
            refresh.AddCssClass("tech-captcha-refresh");
            refresh.Attributes["title"] = isEnglish ? "New image" : "تصویر جدید";
            refresh.Attributes["aria-label"] = isEnglish ? "New image" : "تصویر جدید";

            if (_disabled)
            {
                refresh.Attributes["disabled"] = "disabled";
            }

            var icon = new TagBuilder("i");

            icon.AddCssClass("bi bi-arrow-clockwise");
            icon.Attributes["aria-hidden"] = "true";

            refresh.InnerHtml.AppendHtml(icon);
            visual.InnerHtml.AppendHtml(refresh);

            root.InnerHtml.AppendHtml(visual);

            var body = new TagBuilder("div");

            body.AddCssClass("tech-captcha-body");

            var input = new TagBuilder("input");

            input.Attributes["type"] = "text";
            input.Attributes["id"] = _name;
            input.Attributes["name"] = _name;
            input.Attributes["autocomplete"] = "off";
            input.Attributes["spellcheck"] = "false";
            input.AddCssClass("form-control tech-input-control tech-captcha-answer");
            input.Attributes["placeholder"] = _placeholder
                ?? (isEnglish
                    ? (_kind == CaptchaKind.Math
                        ? "Enter the sum"
                        : "Enter the security code")
                    : (_kind == CaptchaKind.Math
                        ? "حاصل جمع را وارد کنید"
                        : "کد امنیتی را وارد کنید"));

            if (isNumeric)
            {
                input.Attributes["inputmode"] = "numeric";
                input.Attributes["dir"] = "ltr";
            }

            if (_disabled)
            {
                input.Attributes["disabled"] = "disabled";
            }

            if (_required)
            {
                input.Attributes["required"] = "required";
            }

            if (!string.IsNullOrWhiteSpace(_error))
            {
                input.AddCssClass("is-invalid");
                input.Attributes["aria-invalid"] = "true";
                input.Attributes["aria-describedby"] = _name + "-error";
                input.Attributes["autofocus"] = "autofocus";
            }

            body.InnerHtml.AppendHtml(input);

            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = tokenName;
            hidden.Attributes["value"] = challenge.Token;

            body.InnerHtml.AppendHtml(hidden);

            if (!string.IsNullOrWhiteSpace(_error))
            {
                var error = new TagBuilder("span");

                error.AddCssClass("tech-captcha-error");
                error.Attributes["id"] = _name + "-error";
                error.Attributes["role"] = "alert";
                error.InnerHtml.Append(_error);

                body.InnerHtml.AppendHtml(error);
            }

            root.InnerHtml.AppendHtml(body);

            root.WriteTo(writer, encoder);
        }
    }
}
