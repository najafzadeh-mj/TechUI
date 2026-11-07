using System.Collections.Concurrent;
using TechUI.Mvc.Chat;

namespace TechUI.SampleMvc.Services;

/// <summary>
/// In-memory inbox used by the sample support chat. A real application
/// would replace it with a database + SignalR (or any message hub);
/// the chat component itself only needs the request/response contracts.
/// </summary>
public sealed class SupportChatStore
{
    private const int ReplyDelaySeconds = 2;
    private const int MaxEntriesPerConversation = 200;

    private readonly ConcurrentDictionary<string, List<Entry>> _conversations = new();
    private long _lastId;

    public long AddUserMessage(string conversationId, string content)
    {
        var list = _conversations.GetOrAdd(
            Normalize(conversationId), _ => new List<Entry>());

        lock (list)
        {
            var entry = new Entry
            {
                Id = Interlocked.Increment(ref _lastId),
                Role = "user",
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            list.Add(entry);

            if (list.Count > MaxEntriesPerConversation)
            {
                list.RemoveRange(0, list.Count - MaxEntriesPerConversation);
            }

            return entry.Id;
        }
    }

    public TechChatResponse Poll(string conversationId, long after)
    {
        var response = new TechChatResponse();

        if (!_conversations.TryGetValue(
            Normalize(conversationId), out var list))
        {
            return response;
        }

        lock (list)
        {
            var lastUser = list.LastOrDefault(entry => entry.Role == "user");
            var lastAgent = list.LastOrDefault(entry => entry.Role == "agent");

            var awaitingReply = lastUser is not null &&
                (lastAgent is null || lastUser.Id > lastAgent.Id);

            if (awaitingReply)
            {
                var elapsed = DateTime.UtcNow - lastUser!.CreatedAt;

                if (elapsed.TotalSeconds >= ReplyDelaySeconds)
                {
                    list.Add(new Entry
                    {
                        Id = Interlocked.Increment(ref _lastId),
                        Role = "agent",
                        Sender = "مریم · کارشناس پشتیبانی",
                        Content = BuildAgentReply(lastUser.Content),
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    response.Typing = true;
                }
            }

            response.Messages = list
                .Where(entry => entry.Id > after && entry.Role != "user")
                .Select(entry => new TechChatMessage
                {
                    Id = entry.Id,
                    Role = entry.Role,
                    Sender = entry.Sender,
                    Content = entry.Content
                })
                .ToList();
        }

        return response;
    }

    private static string Normalize(string? conversationId)
    {
        return string.IsNullOrWhiteSpace(conversationId)
            ? "anonymous"
            : conversationId.Trim();
    }

    private static string BuildAgentReply(string message)
    {
        var text = message.ToLowerInvariant();

        if (text.Contains("سفارش") || text.Contains("پیگیری"))
        {
            return "کد رهگیری سفارش را برایم بفرستید تا وضعیت مرسوله را بررسی کنم. " +
                "اگر کد را ندارید، شمارهٔ موبایل ثبت‌شده در سفارش کافی است.";
        }

        if (text.Contains("مرجوع") || text.Contains("بازگشت") || text.Contains("تعویض"))
        {
            return "تا ۷ روز پس از تحویل می‌توانید کالا را بدون آسیب‌دیدگی مرجوع کنید. " +
                "بسته‌بندی اصلی و فاکتور را همراه داشته باشید.";
        }

        if (text.Contains("قیمت") || text.Contains("هزینه") || text.Contains("تخفیف"))
        {
            return "قیمت‌های سایت به‌روز هستند و تخفیف‌های فعال در صفحهٔ محصول نمایش داده می‌شوند. " +
                "برای خرید عمده، پیش‌فاکتور را برایتان صادر می‌کنم.";
        }

        if (text.Contains("ساعت") || text.Contains("کاری") || text.Contains("زمان پاسخ"))
        {
            return "تیم پشتیبانی شنبه تا چهارشنبه از ۹ تا ۱۸ و پنجشنبه‌ها تا ۱۳ پاسخگوست؛ " +
                "بیرون از این بازه پیام‌ها صف‌بندی می‌شوند.";
        }

        if (text.Contains("سلام") || text.Contains("درود"))
        {
            return "سلام! خوش آمدید. در خدمتم — موضوع پیامتان را بنویسید تا راهنمایی کنم.";
        }

        return "پیام شما ثبت شد. کارشناس پشتیبانی به‌زودی پاسخ می‌دهد؛ " +
            "اگر فوری است، شمارهٔ سفارش یا کد رهگیری را هم بفرستید.";
    }

    private sealed class Entry
    {
        public long Id { get; init; }
        public string Role { get; init; } = "user";
        public string? Sender { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}