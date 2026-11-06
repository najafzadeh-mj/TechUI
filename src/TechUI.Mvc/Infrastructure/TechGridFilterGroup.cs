using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Infrastructure
{
    public sealed class TechGridFilterGroup
    {
        /// <summary>
        /// and / or
        /// </summary>
        public string Logic { get; set; } = "and";

        public List<TechGridFilter> Filters { get; set; } = new();
    }
}
