using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    public sealed class GridEventOptions
    {
        public string? DataBinding { get; set; }

        public string? DataBound { get; set; }

        public string? Change { get; set; }

        public string? RowClick { get; set; }

        public string? RowDoubleClick { get; set; }

        public string? GroupToggle { get; set; }

        public string? Error { get; set; }
    }
}
