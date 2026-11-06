using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.Infrastructure
{
    public sealed class TechGridRequest
    {
        public const int MaxPageSize = 1000;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        // قدیمی‌ها را نگه می‌داریم برای سازگاری
        public string? SortField { get; set; }
        public string? SortDir { get; set; }

        // جدید
        public List<TechGridSort> Sorts { get; set; } = new();

        public TechGridFilterGroup Filter { get; set; } = new();

    }
}
