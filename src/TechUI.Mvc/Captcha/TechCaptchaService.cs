using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace TechUI.Mvc.Captcha
{
    /// <summary>
    /// Creates stateless captcha challenges and validates submitted answers.
    /// </summary>
    public interface ITechCaptchaService
    {
        TechCaptchaChallenge Create(TechCaptchaRequest request);

        TechCaptchaValidationResult Validate(string? answer, string? token);
    }

    /// <summary>
    /// Generates an encrypted token (answer + expiry) plus an SVG image, so no
    /// per-user server state or session is required.
    /// </summary>
    public sealed class TechCaptchaService : ITechCaptchaService
    {
        private const string EnglishLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string PersianLetters = "ابپتثجچحخدذرزژسشصضطظعغفقکگلمنوهی";
        private const string DigitSet = "0123456789";

        private static readonly string[] NoiseColors =
        {
            "#cbd5e1", "#94a3b8", "#bfdbfe", "#fecaca", "#fde68a"
        };

        private static readonly string[] GlyphColors =
        {
            "#1e293b", "#0f172a", "#334155", "#1f2937"
        };

        private readonly TechCaptchaOptions _options;
        private readonly byte[] _key;

        public TechCaptchaService(IOptions<TechCaptchaOptions> options)
        {
            _options = options.Value;

            var secret = string.IsNullOrWhiteSpace(_options.Secret)
                ? Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
                : _options.Secret;

            _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
        }

        public TechCaptchaChallenge Create(TechCaptchaRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var length = request.Kind == CaptchaKind.Math
                ? 0
                : Math.Clamp(
                    request.Length <= 0 ? 5 : request.Length,
                    3,
                    8);

            var (display, answer) = BuildChallenge(request, length);

            var lifetime = _options.ExpiresIn > TimeSpan.Zero
                ? _options.ExpiresIn
                : TimeSpan.FromMinutes(5);

            var expiresAt = DateTime.UtcNow.Add(lifetime);

            var noise = request.Noise ?? _options.Noise;

            var payload = string.Join(
                '|',
                "1",
                request.Language == CaptchaLanguage.English ? "en" : "fa",
                ToCode(request.Kind),
                answer,
                new DateTimeOffset(expiresAt.ToUniversalTime())
                    .ToUnixTimeSeconds()
                    .ToString(CultureInfo.InvariantCulture));

            return new TechCaptchaChallenge
            {
                Token = Protect(payload),
                ImageDataUri = BuildSvg(display, request.Language, noise),
                ExpiresAtUtc = expiresAt
            };
        }

        public TechCaptchaValidationResult Validate(string? answer, string? token)
        {
            if (string.IsNullOrWhiteSpace(token) ||
                !TryUnprotect(token, out var payload))
            {
                return TechCaptchaValidationResult.Fail(
                    "کد امنیتی نامعتبر است. صفحه را دوباره بارگذاری کنید.");
            }

            var parts = payload.Split('|');

            if (parts.Length != 5 || parts[0] != "1" ||
                !long.TryParse(
                    parts[4],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var expires))
            {
                return TechCaptchaValidationResult.Fail(
                    "کد امنیتی نامعتبر است. صفحه را دوباره بارگذاری کنید.");
            }

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expires)
            {
                return TechCaptchaValidationResult.Fail(
                    "زمان کد امنیتی به پایان رسیده است. کد جدید بگیرید.");
            }

            if (string.IsNullOrWhiteSpace(answer))
            {
                return TechCaptchaValidationResult.Fail(
                    "کد امنیتی را وارد کنید.");
            }

            var expected = Normalize(parts[3]);
            var actual = Normalize(answer);

            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                return TechCaptchaValidationResult.Fail(
                    "کد امنیتی وارد شده صحیح نیست.");
            }

            return TechCaptchaValidationResult.Success;
        }

        private static (string Display, string Answer) BuildChallenge(
            TechCaptchaRequest request,
            int length)
        {
            if (request.Kind == CaptchaKind.Math)
            {
                return BuildMath(request);
            }

            var source = request.Kind == CaptchaKind.Digits
                ? DigitSet
                : request.Language == CaptchaLanguage.Persian
                    ? PersianLetters
                    : EnglishLetters;

            var chars = new char[length];

            for (var index = 0; index < chars.Length; index++)
            {
                chars[index] = source[RandomNumberGenerator.GetInt32(source.Length)];
            }

            var answer = new string(chars);

            var display = request.Kind == CaptchaKind.Letters &&
                request.Language == CaptchaLanguage.Persian
                ? string.Join(" ", chars)
                : LocalizeDigits(answer, request.Language);

            return (display, answer);
        }

        private static (string Display, string Answer) BuildMath(
            TechCaptchaRequest request)
        {
            int left;
            int right;
            int result;
            string symbol;

            switch (request.Operation)
            {
                case CaptchaMathOperation.Subtraction:
                    left = RandomNumberGenerator.GetInt32(6, 13);
                    right = RandomNumberGenerator.GetInt32(2, left);
                    result = left - right;
                    symbol = "−";
                    break;

                case CaptchaMathOperation.Multiplication:
                    left = RandomNumberGenerator.GetInt32(2, 10);
                    right = RandomNumberGenerator.GetInt32(2, 10);
                    result = left * right;
                    symbol = "×";
                    break;

                default:
                    left = RandomNumberGenerator.GetInt32(2, 10);
                    right = RandomNumberGenerator.GetInt32(2, 10);
                    result = left + right;
                    symbol = "+";
                    break;
            }

            var leftText = LocalizeDigits(
                left.ToString(CultureInfo.InvariantCulture),
                request.Language);

            var rightText = LocalizeDigits(
                right.ToString(CultureInfo.InvariantCulture),
                request.Language);

            return (
                $"{leftText} {symbol} {rightText} = ?",
                result.ToString(CultureInfo.InvariantCulture));
        }

        private string BuildSvg(
            string text,
            CaptchaLanguage language,
            CaptchaNoiseLevel noise)
        {
            var width = Math.Clamp(_options.ImageWidth, 140, 400);
            var height = Math.Clamp(_options.ImageHeight, 44, 120);
            var fontSize = text.Length > 10 ? 22 : text.Length > 7 ? 25 : 29;
            var centerY = height / 2;

            var (lineCount, dotCount, curveCount, lineOpacity, dotOpacity,
                curveOpacity, finalOpacity) = noise switch
                {
                    CaptchaNoiseLevel.None => (0, 0, 0, "0", "0", "0", "0"),
                    CaptchaNoiseLevel.Low => (4, 14, 2, "0.3", "0.4", "0.28", "0.25"),
                    CaptchaNoiseLevel.High => (16, 55, 7, "0.6", "0.65", "0.5", "0.45"),
                    _ => (9, 30, 4, "0.45", "0.55", "0.38", "0.35")
                };

            var yJitter = noise switch
            {
                CaptchaNoiseLevel.Low => 0.09,
                CaptchaNoiseLevel.High => 0.2,
                CaptchaNoiseLevel.None => 0,
                _ => 0.14
            };

            var rotationRange = noise switch
            {
                CaptchaNoiseLevel.Low => 7,
                CaptchaNoiseLevel.High => 18,
                CaptchaNoiseLevel.None => 0,
                _ => 11
            };

            var sizeJitter = noise switch
            {
                CaptchaNoiseLevel.Low => 0.04,
                CaptchaNoiseLevel.High => 0.11,
                CaptchaNoiseLevel.None => 0,
                _ => 0.07
            };

            var widthJitter = noise switch
            {
                CaptchaNoiseLevel.Low => 0.08,
                CaptchaNoiseLevel.High => 0.22,
                CaptchaNoiseLevel.None => 0,
                _ => 0.14
            };

            var family = language == CaptchaLanguage.Persian
                ? "Tahoma, 'Segoe UI', Arial, sans-serif"
                : "'Courier New', Consolas, monospace";

            var svg = new StringBuilder();

            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\"")
                .Append(" width=\"").Append(width).Append('"')
                .Append(" height=\"").Append(height).Append('"')
                .Append(" viewBox=\"0 0 ").Append(width).Append(' ')
                .Append(height).Append("\">")
                .Append("<rect width=\"").Append(width)
                .Append("\" height=\"").Append(height)
                .Append("\" rx=\"8\" fill=\"#f8fafc\"/>");

            for (var index = 0; index < lineCount; index++)
            {
                svg.Append("<line x1=\"").Append(Random.Shared.Next(width))
                    .Append("\" y1=\"").Append(Random.Shared.Next(height))
                    .Append("\" x2=\"").Append(Random.Shared.Next(width))
                    .Append("\" y2=\"").Append(Random.Shared.Next(height))
                    .Append("\" stroke=\"")
                    .Append(NoiseColors[Random.Shared.Next(NoiseColors.Length)])
                    .Append("\" stroke-width=\"1.2\" opacity=\"")
                    .Append(lineOpacity).Append("\"/>");
            }

            for (var index = 0; index < curveCount; index++)
            {
                svg.Append("<path d=\"M")
                    .Append(Random.Shared.Next(width)).Append(' ')
                    .Append(Random.Shared.Next(height))
                    .Append(" C")
                    .Append(Random.Shared.Next(width)).Append(' ')
                    .Append(Random.Shared.Next(height)).Append(", ")
                    .Append(Random.Shared.Next(width)).Append(' ')
                    .Append(Random.Shared.Next(height)).Append(", ")
                    .Append(Random.Shared.Next(width)).Append(' ')
                    .Append(Random.Shared.Next(height))
                    .Append("\" fill=\"none\" stroke=\"")
                    .Append(NoiseColors[Random.Shared.Next(NoiseColors.Length)])
                    .Append("\" stroke-width=\"1.1\" opacity=\"")
                    .Append(curveOpacity).Append("\"/>");
            }

            for (var index = 0; index < dotCount; index++)
            {
                svg.Append("<circle cx=\"").Append(Random.Shared.Next(width))
                    .Append("\" cy=\"").Append(Random.Shared.Next(height))
                    .Append("\" r=\"").Append(Random.Shared.Next(1, 3))
                    .Append("\" fill=\"")
                    .Append(NoiseColors[Random.Shared.Next(NoiseColors.Length)])
                    .Append("\" opacity=\"").Append(dotOpacity).Append("\"/>");
            }

            var advance = fontSize *
                (language == CaptchaLanguage.Persian ? 0.72 : 0.78);
            var advances = new double[text.Length];
            var total = 0.0;

            for (var index = 0; index < text.Length; index++)
            {
                var factor = char.IsWhiteSpace(text[index])
                    ? 0.55
                    : 1 + Jitter(widthJitter);

                advances[index] = advance * factor;
                total += advances[index];
            }

            var available = width - 20.0;
            var scale = total > available ? available / total : 1.0;
            var cursor = (width - total * scale) / 2.0;

            for (var index = 0; index < text.Length; index++)
            {
                var glyphWidth = advances[index] * scale;

                if (!char.IsWhiteSpace(text[index]))
                {
                    var centerX = cursor + glyphWidth / 2;
                    var glyphSize = fontSize * (1 + Jitter(sizeJitter));
                    var baseline = centerY + glyphSize / 3 +
                        Jitter(fontSize * yJitter);

                    baseline = Math.Clamp(
                        baseline,
                        glyphSize * 0.9,
                        height - 4);

                    var angle = rotationRange == 0
                        ? 0
                        : Random.Shared.Next(-rotationRange, rotationRange + 1);

                    svg.Append("<text x=\"").Append(Format(centerX))
                        .Append("\" y=\"").Append(Format(baseline))
                        .Append("\" text-anchor=\"middle\"")
                        .Append(" font-family=\"").Append(family).Append('"')
                        .Append(" font-size=\"").Append(Format(glyphSize))
                        .Append("\" font-weight=\"700\" fill=\"")
                        .Append(noise == CaptchaNoiseLevel.High
                            ? GlyphColors[Random.Shared.Next(GlyphColors.Length)]
                            : "#1e293b")
                        .Append("\"");

                    if (angle != 0)
                    {
                        svg.Append(" transform=\"rotate(").Append(angle)
                            .Append(' ').Append(Format(centerX)).Append(' ')
                            .Append(Format(baseline - glyphSize / 3))
                            .Append(")\"");
                    }

                    svg.Append('>')
                        .Append(WebUtility.HtmlEncode(text[index].ToString()))
                        .Append("</text>");
                }

                cursor += glyphWidth;
            }

            if (noise != CaptchaNoiseLevel.None)
            {
                svg.Append("<line x1=\"").Append(Random.Shared.Next(10, 40))
                    .Append("\" y1=\"").Append(Random.Shared.Next(height / 2, height))
                    .Append("\" x2=\"").Append(Random.Shared.Next(width - 40, width - 10))
                    .Append("\" y2=\"").Append(Random.Shared.Next(0, height / 2))
                    .Append("\" stroke=\"#64748b\" stroke-width=\"1\" opacity=\"")
                    .Append(finalOpacity).Append("\"/>");
            }

            svg.Append("</svg>");

            return "data:image/svg+xml;base64," +
                Convert.ToBase64String(Encoding.UTF8.GetBytes(svg.ToString()));
        }

        private static double Jitter(double amount)
        {
            return (Random.Shared.NextDouble() * 2 - 1) * amount;
        }

        private static string Format(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string ToCode(CaptchaKind kind)
        {
            return kind switch
            {
                CaptchaKind.Digits => "digits",
                CaptchaKind.Math => "math",
                _ => "letters"
            };
        }

        private static string LocalizeDigits(string value, CaptchaLanguage language)
        {
            if (language != CaptchaLanguage.Persian)
            {
                return value;
            }

            var buffer = new StringBuilder(value.Length);

            foreach (var ch in value)
            {
                buffer.Append(ch >= '0' && ch <= '9'
                    ? (char)('\u06F0' + (ch - '0'))
                    : ch);
            }

            return buffer.ToString();
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var buffer = new StringBuilder(value.Length);

            foreach (var original in value)
            {
                var ch = original;

                if (ch >= '\u06F0' && ch <= '\u06F9')
                {
                    ch = (char)('0' + (ch - '\u06F0'));
                }
                else if (ch >= '\u0660' && ch <= '\u0669')
                {
                    ch = (char)('0' + (ch - '\u0660'));
                }

                if (char.IsWhiteSpace(ch) || ch == '\u200C' || ch == '\u200D')
                {
                    continue;
                }

                if (ch == 'ي')
                {
                    ch = 'ی';
                }
                else if (ch == 'ك')
                {
                    ch = 'ک';
                }

                buffer.Append(char.ToUpperInvariant(ch));
            }

            return buffer.ToString();
        }

        private string Protect(string payload)
        {
            var nonce = RandomNumberGenerator.GetBytes(12);
            var plain = Encoding.UTF8.GetBytes(payload);
            var cipher = new byte[plain.Length];
            var tag = new byte[16];

            using (var aes = new AesGcm(_key, 16))
            {
                aes.Encrypt(nonce, plain, cipher, tag);
            }

            var buffer = new byte[nonce.Length + cipher.Length + tag.Length];

            Buffer.BlockCopy(nonce, 0, buffer, 0, nonce.Length);
            Buffer.BlockCopy(cipher, 0, buffer, nonce.Length, cipher.Length);
            Buffer.BlockCopy(
                tag,
                0,
                buffer,
                nonce.Length + cipher.Length,
                tag.Length);

            return Convert.ToBase64String(buffer)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private bool TryUnprotect(string token, out string payload)
        {
            payload = string.Empty;

            byte[] data;

            try
            {
                var encoded = token.Replace('-', '+').Replace('_', '/');

                encoded = (encoded.Length % 4) switch
                {
                    2 => encoded + "==",
                    3 => encoded + "=",
                    1 => string.Empty,
                    _ => encoded
                };

                if (encoded.Length == 0)
                {
                    return false;
                }

                data = Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                return false;
            }

            if (data.Length <= 12 + 16)
            {
                return false;
            }

            var nonce = data.AsSpan(0, 12);
            var tag = data.AsSpan(data.Length - 16, 16);
            var cipher = data.AsSpan(12, data.Length - 28);
            var plain = new byte[cipher.Length];

            try
            {
                using var aes = new AesGcm(_key, 16);

                aes.Decrypt(nonce, cipher, tag, plain);
            }
            catch (CryptographicException)
            {
                return false;
            }

            payload = Encoding.UTF8.GetString(plain);

            return true;
        }
    }
}
