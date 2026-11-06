using System.ComponentModel.DataAnnotations;
using TechUI.SampleAdmin.Models;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class OrderFormModel
{
    public int Id { get; set; }

    public string? Number { get; set; }

    [Display(Name = "مشتری")]
    [Required(ErrorMessage = "انتخاب مشتری الزامی است.")]
    public int? CustomerId { get; set; }

    [Display(Name = "تاریخ سفارش")]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [Display(Name = "وضعیت سفارش")]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Display(Name = "مبلغ کل")]
    [Range(10_000, 5_000_000_000, ErrorMessage = "مبلغ سفارش باید بین ۱۰٬۰۰۰ و ۵٬۰۰۰٬۰۰۰٬۰۰۰ باشد.")]
    public decimal Total { get; set; }

    [Display(Name = "یادداشت")]
    [StringLength(1000, ErrorMessage = "طول یادداشت نباید بیشتر از ۱۰۰۰ کاراکتر باشد.")]
    public string? Note { get; set; }
}
