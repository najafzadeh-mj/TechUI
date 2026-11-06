using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Html
{

    public static class TechHtmlExtensions
    {
        /// <summary>
        /// Entry point for Tech fluent API.
        /// Usage: @Html.Tech()
        /// </summary>
        public static TechBuilder Tech(this IHtmlHelper html)
        {
            return new TechBuilder(html);
        }
    }
}
