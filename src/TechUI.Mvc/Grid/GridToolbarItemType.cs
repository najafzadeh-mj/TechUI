using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Built-in toolbar item types for TechUI Grid.
    /// </summary>
    public enum GridToolbarItemType
    {
        /// <summary>
        /// Create/Add new record button.
        /// </summary>
        Create,

        /// <summary>
        /// Refresh grid data button.
        /// </summary>
        Refresh,

        /// <summary>
        /// Show column chooser popup.
        /// </summary>
        ColumnChooser,

        /// <summary>
        /// Custom user-defined toolbar button.
        /// </summary>
        Custom
    }
}
