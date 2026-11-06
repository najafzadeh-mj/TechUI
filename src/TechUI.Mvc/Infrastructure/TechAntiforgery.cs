using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace TechUI.Mvc.Infrastructure;

internal static class TechAntiforgery
{
    public static string? GetToken(IHtmlHelper html)
    {
        var httpContext = html.ViewContext?.HttpContext;

        if (httpContext == null)
        {
            return null;
        }

        var antiforgery = httpContext.RequestServices
            .GetService<IAntiforgery>();

        return antiforgery?
            .GetAndStoreTokens(httpContext)
            .RequestToken;
    }
}
