using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    public sealed class GridColumn
    {
        public string Key { get; set; } = string.Empty;

        public string Field { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Normalized CLR type used by the client filter popup
        /// (string, number, boolean, date, time, guid, enum).
        /// </summary>
        public string? DataType { get; set; }

        public int? Width { get; set; }

        public bool Sortable { get; set; } = true;

        public bool Filterable { get; set; } = true;

        public bool IsTemplate { get; set; }

        public string? Template { get; set; }

    }
}
