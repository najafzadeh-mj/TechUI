using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace TechUI.Mvc.Infrastructure;

internal static class TechUrl
{
    public static string Action(
        IHtmlHelper html,
        string action,
        string controller,
        string? area = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(controller))
        {
            throw new ArgumentException("Controller is required.", nameof(controller));
        }

        var httpContext = html.ViewContext?.HttpContext;

        if (httpContext != null)
        {
            var factory = httpContext.RequestServices
                .GetService<IUrlHelperFactory>();

            var url = factory?.GetUrlHelper(html.ViewContext!);

            var values = string.IsNullOrWhiteSpace(area)
                ? null
                : new { area };

            var generated = url?.Action(action, controller, values);

            if (!string.IsNullOrWhiteSpace(generated))
            {
                return generated;
            }
        }

        return string.IsNullOrWhiteSpace(area)
            ? $"/{controller}/{action}"
            : $"/{area}/{controller}/{action}";
    }
}
