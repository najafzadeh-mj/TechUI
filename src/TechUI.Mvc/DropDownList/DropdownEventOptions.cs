using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.DropDownList
{
    public sealed class DropdownEventOptions
    {
        public string? Change { get; set; } 
        public string? DataBound { get; set; } 
        public string? Open { get; set; } 
        public string? Close { get; set; } 
    }
}
