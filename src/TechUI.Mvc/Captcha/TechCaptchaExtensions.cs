using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TechUI.Mvc.Captcha
{
    public static class TechCaptchaServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the captcha token service. Configure a stable
        /// <see cref="TechCaptchaOptions.Secret"/> in production.
        /// </summary>
        public static IServiceCollection AddTechUICaptcha(
            this IServiceCollection services,
            Action<TechCaptchaOptions>? configure = null)
        {
            services.AddOptions<TechCaptchaOptions>();

            if (configure is not null)
            {
                services.Configure(configure);
            }

            services.TryAddSingleton<ITechCaptchaService, TechCaptchaService>();

            return services;
        }
    }

    public static class TechCaptchaEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the JSON endpoint used by the captcha refresh button.
        /// </summary>
        public static IEndpointRouteBuilder MapTechUICaptcha(
            this IEndpointRouteBuilder endpoints,
            string pattern = "/techui/captcha")
        {
            endpoints.MapGet(pattern, (
                HttpContext context,
                ITechCaptchaService service) =>
            {
                context.Response.Headers["Cache-Control"] = "no-store";

                var query = context.Request.Query;

                var challenge = service.Create(new TechCaptchaRequest
                {
                    Language = ParseLanguage(query["language"].ToString()),
                    Kind = ParseKind(query["kind"].ToString()),
                    Operation = ParseOperation(query["operation"].ToString()),
                    Length = ParseLength(query["length"].ToString()),
                    Noise = ParseNoise(query["noise"].ToString())
                });

                return Results.Json(new
                {
                    token = challenge.Token,
                    image = challenge.ImageDataUri,
                    expiresAt = challenge.ExpiresAtUtc
                });
            });

            return endpoints;
        }

        private static CaptchaLanguage ParseLanguage(string? value)
        {
            return string.Equals(value, "en", StringComparison.OrdinalIgnoreCase)
                ? CaptchaLanguage.English
                : CaptchaLanguage.Persian;
        }

        private static CaptchaKind ParseKind(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "digits" => CaptchaKind.Digits,
                "math" => CaptchaKind.Math,
                _ => CaptchaKind.Letters
            };
        }

        private static CaptchaMathOperation ParseOperation(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "subtraction" => CaptchaMathOperation.Subtraction,
                "multiplication" => CaptchaMathOperation.Multiplication,
                _ => CaptchaMathOperation.Addition
            };
        }

        private static int ParseLength(string? value)
        {
            return int.TryParse(value, out var length) ? length : 0;
        }

        private static CaptchaNoiseLevel? ParseNoise(string? value)
        {
            return value?.ToLowerInvariant() switch
            {
                "none" => CaptchaNoiseLevel.None,
                "low" => CaptchaNoiseLevel.Low,
                "medium" => CaptchaNoiseLevel.Medium,
                "high" => CaptchaNoiseLevel.High,
                _ => null
            };
        }
    }

    public static class TechCaptchaRequestExtensions
    {
        public const string TokenSuffix = "Token";

        /// <summary>
        /// Validates the posted answer against its signed token. The token field
        /// is named <c>{fieldName}Token</c>.
        /// </summary>
        public static TechCaptchaValidationResult ValidateTechCaptcha(
            this HttpRequest request,
            string fieldName = "Captcha")
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!request.HasFormContentType)
            {
                return TechCaptchaValidationResult.Fail(
                    "فرم کد امنیتی یافت نشد.");
            }

            var form = request.Form;

            return Validate(
                request.HttpContext.RequestServices,
                form[fieldName].ToString(),
                form[fieldName + TokenSuffix].ToString());
        }

        /// <summary>
        /// Async variant for actions that have not read the form yet.
        /// </summary>
        public static async Task<TechCaptchaValidationResult> ValidateTechCaptchaAsync(
            this HttpRequest request,
            string fieldName = "Captcha",
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!request.HasFormContentType)
            {
                return TechCaptchaValidationResult.Fail(
                    "فرم کد امنیتی یافت نشد.");
            }

            var form = await request.ReadFormAsync(cancellationToken);

            return Validate(
                request.HttpContext.RequestServices,
                form[fieldName].ToString(),
                form[fieldName + TokenSuffix].ToString());
        }

        private static TechCaptchaValidationResult Validate(
            IServiceProvider services,
            string answer,
            string token)
        {
            var service = services.GetService<ITechCaptchaService>()
                ?? throw new InvalidOperationException(
                    "TechUI captcha requires services.AddTechUICaptcha().");

            return service.Validate(answer, token);
        }
    }
}
