namespace TechUI.Mvc.Captcha
{
    /// <summary>
    /// Alphabet/digit set used by a TechUI captcha challenge.
    /// </summary>
    public enum CaptchaLanguage
    {
        Persian,
        English
    }

    /// <summary>
    /// Challenge style: random letters, random digits or an arithmetic question.
    /// </summary>
    public enum CaptchaKind
    {
        Letters,
        Digits,
        Math
    }

    /// <summary>
    /// Arithmetic operator used when <see cref="CaptchaKind"/> is Math.
    /// </summary>
    public enum CaptchaMathOperation
    {
        Addition,
        Subtraction,
        Multiplication
    }

    /// <summary>
    /// Amount of visual noise (lines, dots and rotation) over the image.
    /// </summary>
    public enum CaptchaNoiseLevel
    {
        None,
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Server-side captcha configuration.
    /// </summary>
    public sealed class TechCaptchaOptions
    {
        /// <summary>
        /// Signing/encryption secret. Empty means a random per-process secret,
        /// which invalidates outstanding tokens after a restart.
        /// </summary>
        public string Secret { get; set; } = "";

        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromMinutes(5);

        public int ImageWidth { get; set; } = 190;

        public int ImageHeight { get; set; } = 58;

        /// <summary>
        /// Default noise level; a captcha can override it per instance.
        /// </summary>
        public CaptchaNoiseLevel Noise { get; set; } = CaptchaNoiseLevel.Medium;
    }

    /// <summary>
    /// Parameters for a single challenge.
    /// </summary>
    public sealed class TechCaptchaRequest
    {
        public CaptchaLanguage Language { get; set; } = CaptchaLanguage.Persian;

        public CaptchaKind Kind { get; set; } = CaptchaKind.Letters;

        public CaptchaMathOperation Operation { get; set; } =
            CaptchaMathOperation.Addition;

        /// <summary>Letter/digit count; clamped to 3..8. Ignored for math.</summary>
        public int Length { get; set; }

        /// <summary>Overrides <see cref="TechCaptchaOptions.Noise"/> when set.</summary>
        public CaptchaNoiseLevel? Noise { get; set; }
    }

    /// <summary>
    /// Generated challenge handed to the browser.
    /// </summary>
    public sealed class TechCaptchaChallenge
    {
        public required string Token { get; init; }

        public required string ImageDataUri { get; init; }

        public required DateTime ExpiresAtUtc { get; init; }
    }

    /// <summary>
    /// Result of comparing a submitted answer with its signed token.
    /// </summary>
    public readonly record struct TechCaptchaValidationResult(
        bool IsValid,
        string? Error)
    {
        public static TechCaptchaValidationResult Success { get; } =
            new(true, null);

        public static TechCaptchaValidationResult Fail(string error) =>
            new(false, error);
    }
}
