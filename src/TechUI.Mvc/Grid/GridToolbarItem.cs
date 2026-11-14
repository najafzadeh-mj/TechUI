using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    public sealed class GridToolbarItem
    {
        public GridToolbarItemType Type { get; set; }

        public string Text { get; set; } = string.Empty;

        public string? Handler { get; set; }

        public string? Icon { get; set; }

        public string? FileName { get; set; }
    }
}
