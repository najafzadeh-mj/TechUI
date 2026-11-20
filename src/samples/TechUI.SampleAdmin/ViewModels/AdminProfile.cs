using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class AdminProfile
{
    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "وارد کردن نام الزامی است.")]
    [StringLength(80, ErrorMessage = "طول نام نباید بیشتر از ۸۰ کاراکتر باشد.")]
    public string FullName { get; set; } = "مدیر فروشگاه";

    [Display(Name = "عنوان نقش")]
    [Required(ErrorMessage = "وارد کردن عنوان نقش الزامی است.")]
    [StringLength(60, ErrorMessage = "طول عنوان نقش نباید بیشتر از ۶۰ کاراکتر باشد.")]
    public string RoleTitle { get; set; } = "مدیر سیستم";

    [Display(Name = "ایمیل")]
    [Required(ErrorMessage = "وارد کردن ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل معتبر نیست.")]
    public string Email { get; set; } = "admin@techui.sample";

    [Display(Name = "شماره تماس")]
    [StringLength(20, ErrorMessage = "طول شماره تماس نباید بیشتر از ۲۰ کاراکتر باشد.")]
    public string? Phone { get; set; } = "0912 000 0000";

    [Display(Name = "شهر")]
    [StringLength(60, ErrorMessage = "طول نام شهر نباید بیشتر از ۶۰ کاراکتر باشد.")]
    public string? City { get; set; } = "تهران";

    [Display(Name = "دربارهٔ من")]
    [StringLength(400, ErrorMessage = "طول متن دربارهٔ من نباید بیشتر از ۴۰۰ کاراکتر باشد.")]
    public string? Bio { get; set; } =
        "نمونهٔ صفحهٔ پروفایل برای نمایش دادهٔ واقعی کاربر در پنل مدیریت TechUI.";

    [JsonIgnore]
    public string AvatarLetter =>
        string.IsNullOrWhiteSpace(FullName)
            ? "م"
            : FullName.Trim()[..1];
}
