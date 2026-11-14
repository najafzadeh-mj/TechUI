using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Grid
{
    /// <summary>
    /// Fluent builder for grid client-side events.
    /// Usage:
    /// .Events(e => e.Change("onGridChange"))
    /// </summary>
    public sealed class GridEventBuilder
    {
        private readonly GridEventOptions _options;

        internal GridEventBuilder(GridEventOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Fires before data loading starts.
        /// </summary>
        public GridEventBuilder DataBinding(string handler)
        {
            _options.DataBinding = handler;
            return this;
        }

        /// <summary>
        /// Fires after data is loaded and rendered.
        /// </summary>
        public GridEventBuilder DataBound(string handler)
        {
            _options.DataBound = handler;
            return this;
        }

        /// <summary>
        /// Fires when selected row changes.
        /// </summary>
        public GridEventBuilder Change(string handler)
        {
            _options.Change = handler;
            return this;
        }

        /// <summary>
        /// Fires when a data row is clicked. Ignored for clicks on
        /// links, buttons and form controls inside the row.
        /// </summary>
        public GridEventBuilder RowClick(string handler)
        {
            _options.RowClick = handler;
            return this;
        }

        /// <summary>
        /// Fires when a data row is double-clicked.
        /// </summary>
        public GridEventBuilder RowDoubleClick(string handler)
        {
            _options.RowDoubleClick = handler;
            return this;
        }

        /// <summary>
        /// Fires when a group is expanded or collapsed. The payload contains
        /// the group key and the new collapsed state.
        /// </summary>
        public GridEventBuilder GroupToggle(string handler)
        {
            _options.GroupToggle = handler;
            return this;
        }

        /// <summary>
        /// Fires when an error occurs during data loading.
        /// </summary>
        public GridEventBuilder Error(string handler)
        {
            _options.Error = handler;
            return this;
        }
    }
}
