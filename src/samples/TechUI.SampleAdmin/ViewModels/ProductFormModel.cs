using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class ProductFormModel
{
    public int Id { get; set; }

    [Display(Name = "نام محصول")]
    [Required(ErrorMessage = "وارد کردن نام محصول الزامی است.")]
    [StringLength(160, ErrorMessage = "طول نام محصول نباید بیشتر از ۱۶۰ کاراکتر باشد.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "کد کالا")]
    [Required(ErrorMessage = "وارد کردن کد کالا الزامی است.")]
    [StringLength(40, ErrorMessage = "طول کد کالا نباید بیشتر از ۴۰ کاراکتر باشد.")]
    public string Sku { get; set; } = string.Empty;

    [Display(Name = "دسته‌بندی")]
    [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است.")]
    public int? CategoryId { get; set; }

    [Display(Name = "قیمت")]
    [Range(1_000, 5_000_000_000, ErrorMessage = "قیمت باید بین ۱٬۰۰۰ و ۵٬۰۰۰٬۰۰۰٬۰۰۰ باشد.")]
    public decimal Price { get; set; }

    [Display(Name = "موجودی")]
    [Range(0, 100_000, ErrorMessage = "موجودی باید بین ۰ و ۱۰۰٬۰۰۰ باشد.")]
    public int Stock { get; set; }

    [Display(Name = "تاریخ ثبت")]
    public DateTime CreatedOn { get; set; } = DateTime.Today;

    [Display(Name = "وضعیت فعال")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "توضیحات")]
    [StringLength(4000, ErrorMessage = "طول توضیحات نباید بیشتر از ۴۰۰۰ کاراکتر باشد.")]
    public string? Description { get; set; }
}
