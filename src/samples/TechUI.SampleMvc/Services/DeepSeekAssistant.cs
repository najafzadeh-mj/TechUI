using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TechUI.Mvc.Chat;

namespace TechUI.SampleMvc.Services;

/// <summary>
/// Options for the DeepSeek chat-completions API. The key is read from
/// configuration (<c>DeepSeek:ApiKey</c>) or the <c>DEEPSEEK_API_KEY</c>
/// environment variable and must never be committed to source control.
/// </summary>
public sealed class DeepSeekOptions
{
    public string? ApiKey { get; set; }

    public string BaseUrl { get; set; } = "https://api.deepseek.com";

    public string Model { get; set; } = "deepseek-chat";

    public int TimeoutSeconds { get; set; } = 60;

    public int MaxTokens { get; set; } = 1024;

    public string? SystemPrompt { get; set; }
}

public readonly record struct DeepSeekAnswer(bool Success, string? Reply, string? Error);

/// <summary>
/// Minimal OpenAI-compatible client for DeepSeek. Returns a structured result
/// instead of throwing so the caller can fall back to <see cref="LocalAssistant"/>
/// when no key is configured or the provider is unreachable.
/// </summary>
public sealed class DeepSeekAssistant
{
    private const string DefaultSystemPrompt =
        "شما «دستیار TechUI» هستید؛ دستیار فارسی‌زبان یک فروشگاه اینترنتی نمونه. " +
        "پاسخ‌ها را کوتاه، دقیق و محاوره‌ای بدهید و از حباب‌های Markdown سنگین " +
        "پرهیز کنید. اگر اطلاعات کافی نیست، یک سؤال روشن‌کننده بپرسید.";

    private const int MaxHistoryMessages = 24;
    private const int MaxMessageLength = 4000;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly DeepSeekOptions _options;
    private readonly ILogger<DeepSeekAssistant> _logger;

    public DeepSeekAssistant(
        HttpClient http,
        IOptions<DeepSeekOptions> options,
        ILogger<DeepSeekAssistant> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (!Uri.TryCreate(
                _options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException(
                "DeepSeek:BaseUrl must be an absolute URL.");
        }

        _http = http;
        _http.BaseAddress = baseUri;
        _http.Timeout = TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 5, 120));
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public string Model => _options.Model;

    public async Task<DeepSeekAnswer> ReplyAsync(
        TechChatRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsConfigured)
        {
            return new DeepSeekAnswer(false, null, "DeepSeek API key is not configured.");
        }

        var payload = new Dictionary<string, object?>
        {
            ["model"] = _options.Model,
            ["messages"] = BuildMessages(request),
            ["stream"] = false,
            ["temperature"] = 0.7,
            ["max_tokens"] = Math.Clamp(_options.MaxTokens, 64, 8192)
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(payload, options: JsonOptions)
        };

        httpRequest.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        try
        {
            using var response = await _http.SendAsync(httpRequest, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var providerError = ReadError(body);
                _logger.LogWarning(
                    "DeepSeek returned {Status}: {Error}",
                    (int)response.StatusCode,
                    providerError);

                return new DeepSeekAnswer(
                    false,
                    null,
                    $"HTTP {(int)response.StatusCode}{(providerError.Length > 0 ? " · " + providerError : string.Empty)}");
            }

            var completion = JsonSerializer.Deserialize<CompletionResponse>(body, JsonOptions);
            var reply = completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrEmpty(reply))
            {
                _logger.LogWarning("DeepSeek returned an empty completion.");
                return new DeepSeekAnswer(false, null, "empty completion");
            }

            return new DeepSeekAnswer(true, reply, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "DeepSeek request failed.");

            return new DeepSeekAnswer(
                false,
                null,
                ex is TaskCanceledException ? "request timed out" : ex.Message);
        }
    }

    private List<DeepSeekMessage> BuildMessages(TechChatRequest request)
    {
        var messages = new List<DeepSeekMessage>
        {
            new("system", Prompt())
        };

        if (request.History is { Count: > 0 })
        {
            foreach (var message in request.History.TakeLast(MaxHistoryMessages))
            {
                var role = (message.Role ?? string.Empty).Trim().ToLowerInvariant();

                if (role is not ("user" or "assistant"))
                {
                    continue;
                }

                var content = Trim(message.Content);

                if (content.Length > 0)
                {
                    messages.Add(new DeepSeekMessage(role, content));
                }
            }
        }

        messages.Add(new DeepSeekMessage("user", Trim(request.Message)));

        return messages;
    }

    private string Prompt()
    {
        return string.IsNullOrWhiteSpace(_options.SystemPrompt)
            ? DefaultSystemPrompt
            : _options.SystemPrompt.Trim();
    }

    private static string Trim(string? value)
    {
        var text = (value ?? string.Empty).Trim();

        return text.Length <= MaxMessageLength
            ? text
            : text[..MaxMessageLength];
    }

    private static string ReadError(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);

            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String)
            {
                return Trim(message.GetString());
            }
        }
        catch (JsonException)
        {
            // Fall through to the raw body.
        }

        return Trim(body);
    }

    private sealed record DeepSeekMessage(string Role, string Content);

    private sealed class CompletionResponse
    {
        public List<CompletionChoice>? Choices { get; set; }
    }

    private sealed class CompletionChoice
    {
        public CompletionMessage? Message { get; set; }
    }

    private sealed class CompletionMessage
    {
        public string? Content { get; set; }
    }
}