using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class AdminSettings
{
    [Display(Name = "نام فروشگاه")]
    [Required(ErrorMessage = "وارد کردن نام فروشگاه الزامی است.")]
    [StringLength(80, ErrorMessage = "طول نام فروشگاه نباید بیشتر از ۸۰ کاراکتر باشد.")]
    public string StoreName { get; set; } = "فروشگاه تک‌شاپ";

    [Display(Name = "ایمیل پشتیبانی")]
    [Required(ErrorMessage = "وارد کردن ایمیل پشتیبانی الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل پشتیبانی معتبر نیست.")]
    public string SupportEmail { get; set; } = "support@techui.sample";

    [Display(Name = "واحد پول")]
    [Required(ErrorMessage = "انتخاب واحد پول الزامی است.")]
    public string Currency { get; set; } = "ریال";

    [Display(Name = "تعداد ردیف در هر صفحه")]
    [Range(5, 100, ErrorMessage = "تعداد ردیف باید بین ۵ و ۱۰۰ باشد.")]
    public int DefaultPageSize { get; set; } = 15;

    [Display(Name = "فعال بودن اعلان‌ها")]
    public bool EnableNotifications { get; set; } = true;

    [Display(Name = "اعلان سفارش جدید")]
    public bool NotifyNewOrder { get; set; } = true;

    [Display(Name = "اعلان کاهش موجودی")]
    public bool NotifyLowStock { get; set; } = true;

    [Display(Name = "گزارش هفتگی ایمیلی")]
    public bool NotifyWeeklyReport { get; set; }

    [Display(Name = "قالب رنگی")]
    public string ThemePreset { get; set; } = "default";

    [Display(Name = "حالت تاریک")]
    public bool DarkMode { get; set; }

    [Display(Name = "متن پاورقی")]
    [StringLength(200, ErrorMessage = "طول متن پاورقی نباید بیشتر از ۲۰۰ کاراکتر باشد.")]
    public string FooterText { get; set; } = "نمونه‌ی کامل TechUI برای ASP.NET Core MVC";
}
