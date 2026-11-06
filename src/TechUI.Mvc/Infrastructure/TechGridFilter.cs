using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Infrastructure
{
    public sealed class TechGridFilter
    {
        public string Field { get; set; } = string.Empty;

        public string Operator { get; set; } = "contains";

        public string? Value { get; set; }
    }
}
