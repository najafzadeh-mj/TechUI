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

        public int Total { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
