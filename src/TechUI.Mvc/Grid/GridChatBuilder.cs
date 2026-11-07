namespace TechUI.Mvc.Grid;

public sealed class GridChatBuilder
{
    internal GridChatBuilder(GridChatOptions options)
    {
        Options = options;
    }

    internal GridChatOptions Options { get; }

    public GridChatBuilder Url(string url)
    {
        Options.Url = url;
        return this;
    }

    public GridChatBuilder Title(string title)
    {
        Options.Title = title;
        return this;
    }

    public GridChatBuilder Placeholder(string placeholder)
    {
        Options.Placeholder = placeholder;
        return this;
    }

    public GridChatBuilder Welcome(string welcome)
    {
        Options.Welcome = welcome;
        return this;
    }

    public GridChatBuilder ButtonText(string text)
    {
        Options.ButtonText = text;
        return this;
    }

    public GridChatBuilder Icon(string icon)
    {
        Options.Icon = icon.StartsWith("bi-", StringComparison.OrdinalIgnoreCase)
            ? icon[3..]
            : icon;
        return this;
    }

    public GridChatBuilder MaxLength(int maxLength)
    {
        Options.MaxLength = Math.Clamp(maxLength, 1, 4000);
        return this;
    }

    public GridChatBuilder Suggestions(params string[] suggestions)
    {
        Options.Suggestions.Clear();

        if (suggestions is not null)
        {
            foreach (var suggestion in suggestions)
            {
                if (!string.IsNullOrWhiteSpace(suggestion))
                {
                    Options.Suggestions.Add(suggestion.Trim());
                }
            }
        }

        return this;
    }
}

internal sealed class GridChatOptions
{
    public string? Url { get; set; }

    public string Title { get; set; } = "دستیار گرید";

    public string Placeholder { get; set; } = "دستور خود را بنویسید…";

    public string Welcome { get; set; } = "سلام! دربارهٔ داده‌های این جدول بپرسید.";

    public string ButtonText { get; set; } = "دستیار";

    public string Icon { get; set; } = "chat-dots";

    public int MaxLength { get; set; } = 500;

    public List<string> Suggestions { get; } = new();
}
