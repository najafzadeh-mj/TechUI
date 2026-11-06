using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    public sealed class GridColumnBuilder
    {
        private readonly GridColumn _column;

        internal GridColumnBuilder(GridColumn column)
        {
            _column = column;
        }

        public GridColumnBuilder Title(string title)
        {
            _column.Title = title;
            return this;
        }

        public GridColumnBuilder Width(int width)
        {
            _column.Width = width;
            return this;
        }

        public GridColumnBuilder Sortable(bool enabled = true)
        {
            _column.Sortable = enabled;
            return this;
        }

        public GridColumnBuilder Filterable(bool enabled = true)
        {
            _column.Filterable = enabled;
            return this;
        }
    }
}
