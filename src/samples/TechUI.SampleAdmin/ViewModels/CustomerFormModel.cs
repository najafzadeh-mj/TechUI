using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class CustomerFormModel
{
    public int Id { get; set; }

    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "وارد کردن نام و نام خانوادگی الزامی است.")]
    [StringLength(120, ErrorMessage = "طول نام نباید بیشتر از ۱۲۰ کاراکتر باشد.")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "ایمیل")]
    [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست.")]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "شماره موبایل")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید ۱۱ رقم و با ۰۹ شروع شود.")]
    public string? Phone { get; set; }

    [Display(Name = "شهر")]
    [Required(ErrorMessage = "انتخاب شهر الزامی است.")]
    public string City { get; set; } = string.Empty;

    [Display(Name = "اعتبار حساب")]
    [Range(0, 1_000_000_000, ErrorMessage = "مقدار اعتبار باید بین ۰ و ۱٬۰۰۰٬۰۰۰٬۰۰۰ باشد.")]
    public decimal Credit { get; set; }

    [Display(Name = "تاریخ عضویت")]
    public DateTime RegisteredOn { get; set; } = DateTime.Today;

    [Display(Name = "برچسب‌ها")]
    public List<string> Tags { get; set; } = new();

    [Display(Name = "یادداشت")]
    [StringLength(1000, ErrorMessage = "طول یادداشت نباید بیشتر از ۱۰۰۰ کاراکتر باشد.")]
    public string? Notes { get; set; }

    [Display(Name = "وضعیت فعال")]
    public bool IsActive { get; set; } = true;
}
