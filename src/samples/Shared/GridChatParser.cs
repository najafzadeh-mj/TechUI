using System.Text;
using System.Text.RegularExpressions;
using TechUI.Mvc.Grid;

namespace TechUI.Samples.Shared;

/// <summary>
/// Keyless demo "AI" for the grid chat: a small Persian command parser.
/// A real app would call a language model with
/// <c>TechGridChatTools.BuildSystemPrompt</c> instead and return the same
/// <see cref="TechGridChatResponse"/> contract.
/// </summary>
public static class GridChatParser
{
    private static readonly (string Keyword, string Operator)[] FilterKeywords =
    {
        ("بزرگ تر یا مساوی", "gte"),
        ("بزرگتر یا مساوی", "gte"),
        ("کوچک تر یا مساوی", "lte"),
        ("کوچکتر یا مساوی", "lte"),
        ("خالی نیست", "isnotnull"),
        ("خالی است", "isnull"),
        ("بدون مقدار", "isnull"),
        ("شروع شود با", "startswith"),
        ("شروع با", "startswith"),
        ("ختم می شود به", "endswith"),
        ("پایان با", "endswith"),
        ("مساوی نیست", "neq"),
        ("نامساوی", "neq"),
        ("بزرگ تر از", "gt"),
        ("بزرگتر از", "gt"),
        ("بیشتر از", "gt"),
        ("بالاتر از", "gt"),
        ("کوچک تر از", "lt"),
        ("کوچکتر از", "lt"),
        ("کمتر از", "lt"),
        ("پایین تر از", "lt"),
        ("شامل می شود", "contains"),
        ("شامل شود", "contains"),
        ("شامل", "contains"),
        ("مساوی", "eq"),
        ("برابر", "eq")
    };

    private static readonly string[] ClearWords =
    {
        "پاک", "حذف", "بردار", "لغو", "خالی کن", "reset"
    };

    private static readonly string[] HighlightWords =
    {
        "هایلایت", "های لایت", "برجسته", "مشخص کن"
    };

    private static readonly string[] ValueStopWords =
    {
        "فیلتر کنید", "فیلتر کن", "فیلتر", "جستجو کنید", "جستجو کن", "جستجو",
        "جست وجو", "نشان دهید", "نشان بده", "نمایش دهید", "نمایش بده", "نمایش",
        "لیست", "فقط", "مشتریان", "مشتری", "رکوردها", "رکوردهایی", "ردیف ها",
        "ردیفهایی", "هایی که", "هایی", "که", "را", "با", "از", "به", "برای",
        "است", "هستند", "باشد", "باشند", "کنید", "کن", "بکنید", "بکن", "بده", "دهید",
        "انتخاب کنید", "انتخاب کن", "انتخاب", "سلکت",
        "می خواهم", "میخواهم", "تمام", "همه", "مقدار", "هایلایت", "های لایت",
        "برجسته", "رنگ", "مشخص", "بزرگ تر", "بزرگتر", "کوچک تر", "کوچکتر",
        "بیشتر", "کمتر", "بالاتر", "پایین تر", "شامل", "مساوی", "برابر",
        "شروع", "پایان", "ختم", "خالی", "نیست", "صعودی", "نزولی", "مرتب سازی",
        "مرتب", "بر اساس", "اساس"
    };

    private const string HelpText =
        "می‌توانم این کارها را انجام دهم:\n" +
        "• فیلتر: «فیلتر <ستون> <مقدار>» — مثلاً «فیلتر شهر تهران»\n" +
        "• مرتب‌سازی: «مرتب‌سازی بر اساس <ستون> صعودی»\n" +
        "• صفحه‌بندی: «صفحه بعد»، «صفحه ۳» یا «تعداد سطر را ۲۰ کن»\n" +
        "• ستون‌ها: «ستون <نام> را مخفی کن» یا «همه ستون‌ها را نمایش بده»\n" +
        "• هایلایت: «شهر تهران را هایلایت کن»\n" +
        "• انتخاب: «<ستون> <مقدار> را انتخاب کن»\n" +
        "• پاک کردن: «فیلترها را پاک کن»";

    public static TechGridChatResponse Parse(TechGridChatRequest? request)
    {
        var grid = request?.Grid ?? new TechGridChatGrid();

        var message = Normalize(request?.Message);

        if (message.Length == 0)
        {
            return Respond("پیامی دریافت نشد.");
        }

        if (message.Length <= 6 && ContainsAny(message, "سلام", "درود", "hi", "hello"))
        {
            return Respond("سلام! دستور خود را بنویسید. مثلاً «فیلتر شهر تهران» یا «صفحه بعد».");
        }

        if (ContainsAny(message, "کمک", "راهنما", "چه کاری", "چه کارهایی", "دستورها"))
        {
            return Respond(HelpText);
        }

        if (ContainsAny(message, "چند رکورد", "چند ردیف", "چند تا رکورد", "تعداد کل", "کل رکورد"))
        {
            return Respond($"در حال حاضر {grid.Total:N0} رکورد در این جدول وجود دارد.");
        }

        if (ContainsAny(message, "تعداد سطر", "اندازه صفحه", "سایز صفحه", "هر صفحه"))
        {
            return ParsePageSize(message, grid);
        }

        if (message.Contains("صفحه"))
        {
            return ParsePage(message, grid);
        }

        if (ContainsAny(message, HighlightWords))
        {
            return ParseHighlight(message, grid);
        }

        if (message.Contains("انتخاب"))
        {
            return ParseSelect(message, grid);
        }

        if (message.Contains("ستون"))
        {
            return ParseColumns(message, grid);
        }

        if (ContainsAny(message, ClearWords))
        {
            return ParseClear(message);
        }

        if (ContainsAny(message, "مرتب", "سورت", "صعودی", "نزولی"))
        {
            return ParseSort(message, grid);
        }

        var column = FindColumn(grid.Columns, message);

        if (column is not null)
        {
            return ParseFilter(message, grid, column);
        }

        if (ContainsAny(message, "فیلتر", "جستجو", "جست وجو", "شامل", "برابر", "مساوی", "خالی"))
        {
            return Respond("ستون موردنظر را مشخص کنید. مثلاً: «فیلتر شهر تهران» یا «نام کامل شامل User 001».");
        }

        return Respond(HelpText);
    }

    private static TechGridChatResponse ParsePageSize(string message, TechGridChatGrid grid)
    {
        var number = FirstNumber(message);

        if (number is null)
        {
            return Respond(
                $"اندازهٔ صفحهٔ فعلی {grid.State.PageSize} سطر است. " +
                "مثلاً بگویید «تعداد سطر را ۲۰ کن».");
        }

        var size = Math.Clamp(number.Value, 1, 1000);

        return Respond(
            $"اندازهٔ صفحه روی {size} سطر تنظیم شد.",
            new TechGridChatAction { Type = "pageSize", PageSize = size });
    }

    private static TechGridChatResponse ParsePage(string message, TechGridChatGrid grid)
    {
        var current = Math.Max(grid.State.Page, 1);
        var pageSize = Math.Max(grid.State.PageSize, 1);
        var totalPages = Math.Max((int)Math.Ceiling(grid.Total / (double)pageSize), 1);

        int? target = null;

        if (message.Contains("بعد"))
        {
            target = current + 1;
        }
        else if (ContainsAny(message, "قبل", "قبلی", "پیش"))
        {
            target = current - 1;
        }
        else if (message.Contains("اول"))
        {
            target = 1;
        }
        else if (ContainsAny(message, "آخر", "اخر"))
        {
            target = totalPages;
        }
        else
        {
            target = FirstNumber(message);
        }

        if (target is null)
        {
            return Respond(
                $"الان در صفحهٔ {current} از {totalPages} هستید. " +
                "مثلاً بگویید «صفحه بعد» یا «صفحه ۲».");
        }

        var page = Math.Clamp(target.Value, 1, totalPages);

        if (page == current)
        {
            return Respond($"همین حالا در صفحهٔ {current} از {totalPages} هستید.");
        }

        return Respond(
            $"به صفحهٔ {page} از {totalPages} منتقل شدم.",
            new TechGridChatAction { Type = "page", Page = page });
    }

    private static TechGridChatResponse ParseHighlight(string message, TechGridChatGrid grid)
    {
        if (ContainsAny(message, ClearWords))
        {
            return Respond(
                "هایلایت سطرها پاک شد.",
                new TechGridChatAction { Type = "clearHighlight" });
        }

        var column = FindColumn(grid.Columns, message);

        if (column is null)
        {
            return Respond(
                "برای هایلایت، ستون و مقدار را بگویید. " +
                "مثلاً «شهر تهران را هایلایت کن».");
        }

        var value = ExtractValue(message, column, null);

        if (value.Length == 0)
        {
            return Respond($"مقدار موردنظر برای ستون «{column.Title}» را بگویید.");
        }

        return Respond(
            $"سطرهایی که «{column.Title}» آن‌ها «{value}» است هایلایت شدند.",
            new TechGridChatAction
            {
                Type = "highlight",
                Field = column.Field,
                Value = value
            });
    }

    private static TechGridChatResponse ParseSelect(string message, TechGridChatGrid grid)
    {
        var column = FindColumn(grid.Columns, message);

        if (column is null)
        {
            return Respond(
                "برای انتخاب سطر، ستون و مقدار را بگویید. " +
                "مثلاً «نام کامل User 001 را انتخاب کن».");
        }

        var value = ExtractValue(message, column, null);

        if (value.Length == 0)
        {
            return Respond($"مقدار موردنظر برای ستون «{column.Title}» را بگویید.");
        }

        return Respond(
            $"سطر با «{column.Title}» برابر «{value}» انتخاب شد.",
            new TechGridChatAction
            {
                Type = "select",
                Field = column.Field,
                Value = value
            });
    }

    private static TechGridChatResponse ParseColumns(string message, TechGridChatGrid grid)
    {
        var column = FindColumn(grid.Columns, message);

        if (message.Contains("همه"))
        {
            return Respond(
                "همهٔ ستون‌ها نمایش داده شدند.",
                new TechGridChatAction { Type = "showAllColumns" });
        }

        if (ContainsAny(message, "مخفی", "پنهان"))
        {
            if (column is null)
            {
                return Respond("کدام ستون مخفی شود؟ نام ستون را بگویید.");
            }

            return Respond(
                $"ستون «{column.Title}» مخفی شد.",
                new TechGridChatAction
                {
                    Type = "hideColumns",
                    Fields = new List<string> { column.Field }
                });
        }

        if (ContainsAny(message, "نمایش", "نشان", "ظاهر", "باز"))
        {
            if (column is null)
            {
                if (message.Contains("ستون"))
                {
                    return Respond(
                        "همهٔ ستون‌ها نمایش داده شدند.",
                        new TechGridChatAction { Type = "showAllColumns" });
                }

                return Respond("کدام ستون نمایش داده شود؟ نام ستون را بگویید.");
            }

            return Respond(
                $"ستون «{column.Title}» نمایش داده شد.",
                new TechGridChatAction
                {
                    Type = "showColumns",
                    Fields = new List<string> { column.Field }
                });
        }

        return Respond("برای مدیریت ستون‌ها بگویید «ستون شناسه را مخفی کن» یا «همه ستون‌ها را نمایش بده».");
    }

    private static TechGridChatResponse ParseClear(string message)
    {
        var filters = message.Contains("فیلتر");
        var sorts = ContainsAny(message, "مرتب", "سورت");
        var highlight = ContainsAny(message, HighlightWords);

        if (!filters && !sorts && !highlight)
        {
            filters = true;
        }

        var actions = new List<TechGridChatAction>();
        var parts = new List<string>();

        if (filters)
        {
            actions.Add(new TechGridChatAction { Type = "clearFilters" });
            parts.Add("فیلترها");
        }

        if (sorts)
        {
            actions.Add(new TechGridChatAction { Type = "clearSorts" });
            parts.Add("مرتب‌سازی");
        }

        if (highlight)
        {
            actions.Add(new TechGridChatAction { Type = "clearHighlight" });
            parts.Add("هایلایت");
        }

        return Respond(
            $"{string.Join("، ", parts)} پاک شد.",
            actions.ToArray());
    }

    private static TechGridChatResponse ParseSort(string message, TechGridChatGrid grid)
    {
        var column = FindColumn(grid.Columns, message);

        if (column is null)
        {
            return Respond("مرتب‌سازی بر اساس کدام ستون؟ مثلاً «مرتب‌سازی بر اساس نام صعودی».");
        }

        var desc = ContainsAny(message, "نزولی", "desc", "بیشترین", "بزرگ به کوچک", "بالا به پایین");
        var dir = desc ? "desc" : "asc";

        return Respond(
            $"مرتب‌سازی بر اساس «{column.Title}» به‌صورت {(desc ? "نزولی" : "صعودی")} انجام شد.",
            new TechGridChatAction
            {
                Type = "sort",
                Field = column.Field,
                Dir = dir
            });
    }

    private static TechGridChatResponse ParseFilter(
        string message,
        TechGridChatGrid grid,
        TechGridChatColumn column)
    {
        var match = FindOperator(message);

        var op = match?.Operator ?? "contains";

        var value = ExtractValue(message, column, match?.Keyword);

        if (op is "isnull" or "isnotnull")
        {
            return Respond(
                DescribeFilter(column.Title, op, string.Empty),
                new TechGridChatAction
                {
                    Type = "filter",
                    Field = column.Field,
                    Operator = op,
                    Value = string.Empty
                });
        }

        if (value.Length == 0)
        {
            return Respond(
                $"مقدار فیلتر برای ستون «{column.Title}» را بگویید. " +
                $"مثلاً «{column.Title} شامل ...».");
        }

        return Respond(
            DescribeFilter(column.Title, op, value),
            new TechGridChatAction
            {
                Type = "filter",
                Field = column.Field,
                Operator = op,
                Value = value
            });
    }

    private static string DescribeFilter(string title, string op, string value)
    {
        return op switch
        {
            "contains" => $"ستون «{title}» شامل «{value}» فیلتر شد.",
            "eq" => $"ستون «{title}» برابر «{value}» فیلتر شد.",
            "neq" => $"ستون «{title}» نامساوی «{value}» فیلتر شد.",
            "startswith" => $"ستون «{title}» با «{value}» شروع می‌شود.",
            "endswith" => $"ستون «{title}» به «{value}» ختم می‌شود.",
            "gt" => $"ستون «{title}» بزرگ‌تر از «{value}» فیلتر شد.",
            "gte" => $"ستون «{title}» بزرگ‌تر یا مساوی «{value}» فیلتر شد.",
            "lt" => $"ستون «{title}» کوچک‌تر از «{value}» فیلتر شد.",
            "lte" => $"ستون «{title}» کوچک‌تر یا مساوی «{value}» فیلتر شد.",
            "isnull" => $"سطرهای بدون مقدار در ستون «{title}» نمایش داده می‌شوند.",
            "isnotnull" => $"سطرهای دارای مقدار در ستون «{title}» نمایش داده می‌شوند.",
            _ => $"فیلتر ستون «{title}» اعمال شد."
        };
    }

    private static (string Keyword, string Operator)? FindOperator(string message)
    {
        foreach (var item in FilterKeywords)
        {
            if (ContainsWord(message, item.Keyword))
            {
                return item;
            }
        }

        return null;
    }

    private static TechGridChatColumn? FindColumn(
        IEnumerable<TechGridChatColumn>? columns,
        string message)
    {
        TechGridChatColumn? best = null;
        var bestLength = 0;

        foreach (var column in columns ?? Enumerable.Empty<TechGridChatColumn>())
        {
            foreach (var alias in Aliases(column))
            {
                var normalized = Normalize(alias);

                if (normalized.Length < 2) continue;

                if (ContainsWord(message, normalized) && normalized.Length > bestLength)
                {
                    best = column;
                    bestLength = normalized.Length;
                }
            }
        }

        return best;
    }

    private static IEnumerable<string> Aliases(TechGridChatColumn column)
    {
        if (!string.IsNullOrWhiteSpace(column.Title)) yield return column.Title;

        if (!string.IsNullOrWhiteSpace(column.Field)) yield return column.Field;

        foreach (var synonym in Synonyms(column.Field))
        {
            yield return synonym;
        }
    }

    private static string[] Synonyms(string field)
    {
        return field.ToLowerInvariant() switch
        {
            "fullname" => new[] { "نام و نام خانوادگی", "نام کامل", "نام", "اسم" },
            "email" => new[] { "ایمیل", "پست الکترونیک" },
            "city" => new[] { "شهر" },
            "phone" => new[] { "تلفن", "موبایل", "شماره تماس" },
            "isactive" => new[] { "وضعیت", "فعال", "غیرفعال" },
            "credit" => new[] { "اعتبار", "اعتبار حساب", "موجودی", "حساب" },
            "registeredon" => new[] { "تاریخ عضویت", "تاریخ ثبت نام", "عضویت", "ثبت نام" },
            "id" => new[] { "شناسه", "کد" },
            "tags" => new[] { "برچسب", "برچسب ها", "تگ" },
            _ => Array.Empty<string>()
        };
    }

    private static string ExtractValue(
        string message,
        TechGridChatColumn column,
        string? operatorKeyword)
    {
        var value = message;

        if (!string.IsNullOrEmpty(operatorKeyword))
        {
            var index = value.LastIndexOf(operatorKeyword, StringComparison.Ordinal);

            if (index >= 0)
            {
                value = value[(index + operatorKeyword.Length)..];
            }
        }

        foreach (var alias in Aliases(column).OrderByDescending(alias => Normalize(alias).Length))
        {
            var normalized = Normalize(alias);

            if (normalized.Length >= 2)
            {
                value = RemoveWord(value, normalized);
            }
        }

        foreach (var word in ValueStopWords)
        {
            value = RemoveWord(value, word);
        }

        return CleanValue(value);
    }

    private static string CleanValue(string value)
    {
        value = value.Trim();

        value = value.Trim('،', '؛', '؟', '!', '.', ':', '«', '»', '"', '\'', '-', '_', '(', ')');

        value = Regex.Replace(value, @"\s+", " ").Trim();

        value = Regex.Replace(value, @"^(را|که|با|از|به|برای)\s+", string.Empty);

        value = Regex.Replace(value, @"\s+(را|است|هستند|باشد|باشند|کن|کنید)$", string.Empty);

        return value.Trim();
    }

    private static int? FirstNumber(string value)
    {
        var match = Regex.Match(value, @"\d+");

        if (!match.Success) return null;

        return int.TryParse(match.Value, out var number) ? number : null;
    }

    private static bool ContainsAny(string value, params string[] words)
    {
        return words.Any(word => ContainsWord(value, word));
    }

    private static bool ContainsWord(string value, string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return false;

        var pattern =
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(word)}(?![\p{{L}}\p{{N}}])";

        return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
    }

    private static string RemoveWord(string value, string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return value;

        var pattern =
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(word)}(?![\p{{L}}\p{{N}}])";

        return Regex.Replace(value, pattern, " ", RegexOptions.IgnoreCase);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(character switch
            {
                'ي' or 'ى' => 'ی',
                'ك' => 'ک',
                'ة' or 'ۀ' => 'ه',
                'أ' or 'إ' or 'آ' => 'ا',
                'ؤ' => 'و',
                'ئ' => 'ی',
                >= '۰' and <= '۹' => (char)('0' + (character - '۰')),
                >= '٠' and <= '٩' => (char)('0' + (character - '٠')),
                '\u200c' or '\u200e' or '\u200f' => ' ',
                '،' or '؛' or '؟' or '!' or ':' or '«' or '»' or '"' or '\'' or 'ـ' => ' ',
                _ => character
            });
        }

        return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
    }

    private static TechGridChatResponse Respond(
        string reply,
        params TechGridChatAction[] actions)
    {
        return new TechGridChatResponse
        {
            Reply = reply,
            Actions = actions.ToList()
        };
    }
}
