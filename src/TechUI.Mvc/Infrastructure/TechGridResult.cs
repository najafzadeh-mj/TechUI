using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Infrastructure
{
    public sealed class TechGridResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

        /// <summary>
        /// Number of rows, or the number of top level groups when
        /// <see cref="Grouped"/> is <c>true</c>. Paging is computed from it.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Total number of filtered rows. It equals <see cref="Total"/>
        /// unless grouping was applied by the server.
        /// </summary>
        public int? RowTotal { get; set; }

        /// <summary>
        /// True when the server grouped the filtered rows and paged the
        /// top level groups.
        /// </summary>
        public bool Grouped { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        /// <summary>
        /// Aggregates over the filtered data (all pages), in request order.
        /// </summary>
        public List<TechGridSummaryValue> Summary { get; set; } = new();
    }
}
