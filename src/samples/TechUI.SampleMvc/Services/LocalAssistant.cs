using TechUI.Mvc.Chat;

namespace TechUI.SampleMvc.Services;

/// <summary>
/// Keyless local responder used by the sample AI chat. It is intentionally
/// simple: a real assistant would forward <see cref="TechChatRequest"/> to
/// an LLM provider and return its answer in <see cref="TechChatResponse.Reply"/>.
/// </summary>
public static class LocalAssistant
{
    public static string Reply(TechChatRequest request)
    {
        var message = (request.Message ?? string.Empty).Trim();
        var text = message.ToLowerInvariant();

        if (text.Length == 0)
        {
            return "پیام خالی است؛ سؤال خود را بنویسید.";
        }

        if (ContainsAny(text, "سلام", "درود", "hi", "hello"))
        {
            return "سلام! من دستیار نمایشی TechUI هستم (پاسخ محلی و بدون سرویس هوش مصنوعی). " +
                "دربارهٔ قیمت، سفارش، مرجوعی یا ساعات کاری بپرسید.";
        }

        if (ContainsAny(text, "چیکار", "چه کار", "کمک", "راهنما", "help", "قابلیت"))
        {
            return "می‌توانم دربارهٔ این موضوع‌ها راهنمایی کنم:\n" +
                "• قیمت و تخفیف محصولات\n" +
                "• پیگیری سفارش و مرسوله\n" +
                "• شرایط مرجوعی و تعویض\n" +
                "• ساعات کاری و ارتباط با کارشناس";
        }

        if (ContainsAny(text, "ساعت", "کاری", "پاسخگویی", "تعطیل"))
        {
            return "ساعات کاری: شنبه تا چهارشنبه ۹ تا ۱۸ و پنجشنبه ۹ تا ۱۳. " +
                "برای گفتگو با کارشناس انسانی، از پنل «پشتیبانی آنلاین» استفاده کنید.";
        }

        if (ContainsAny(text, "قیمت", "هزینه", "تخفیف", "مبلغ"))
        {
            return "قیمت‌ها روی صفحهٔ هر محصول به‌روز نمایش داده می‌شوند و تخفیف‌های فعال " +
                "با برچسب رنگی مشخص شده‌اند. اگر برای خرید عمده برنامه دارید، بگویید تا " +
                "شرایط پیش‌فاکتور را توضیح دهم.";
        }

        if (ContainsAny(text, "سفارش", "رهگیری", "پیگیری", "مرسوله", "ارسال"))
        {
            return "برای پیگیری سفارش، کد رهگیری را در صفحهٔ «سفارش‌ها» وارد کنید. " +
                "وضعیت‌های ممکن: ثبت‌شده، در حال پردازش، ارسال‌شده و تحویل‌شده.";
        }

        if (ContainsAny(text, "مرجوع", "بازگشت", "تعویض", "پس دادن"))
        {
            return "تا ۷ روز پس از تحویل می‌توانید کالا را با بسته‌بندی سالم و فاکتور مرجوع کنید. " +
                "کالاهای دیجیتال فعال‌شده مشمول مرجوعی نیستند.";
        }

        if (ContainsAny(text, "کارشناس", "پشتیبان", "انسان", "اپراتور"))
        {
            return "پنل «پشتیبانی آنلاین» در همین صفحه به کارشناس واقعی متصل است " +
                "(در نمونه، پاسخ به‌صورت خودکار شبیه‌سازی می‌شود).";
        }

        if (ContainsAny(text, "ممنون", "متشکر", "مرسی", "thanks"))
        {
            return "خواهش می‌کنم! اگر سؤال دیگری داشتید در خدمتم.";
        }

        var historyCount = request.History?.Count ?? 0;

        return $"پیام شما را دریافت کردم: «{message}»\n" +
            "این نسخهٔ نمایشی محلی است و فقط موضوع‌های مشخصی را می‌شناسد. " +
            $"برای اتصال مدل واقعی، همین اکشن را به API دلخواه وصل کنید (تاریخچه: {historyCount} پیام).";
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(keyword => text.Contains(
            keyword, StringComparison.OrdinalIgnoreCase));
    }
}