using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleMvc.Models;

public class ValidationSampleModel
{
    [Display(Name = "نام و نام خانوادگی")]
    [Required]
    [StringLength(40, MinimumLength = 3)]
    public string? FullName { get; set; }

    [Display(Name = "ایمیل")]
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Display(Name = "سن")]
    [Range(18, 99)]
    public int Age { get; set; }

    [Display(Name = "مبلغ")]
    [Range(1000, 50000000)]
    public decimal Price { get; set; }

    [Display(Name = "موبایل")]
    [Required]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید با 09 شروع شود و ۱۱ رقم باشد.")]
    public string? Mobile { get; set; }

    [Display(Name = "تاریخ تولد")]
    [Required]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "ساعت شروع")]
    [Required]
    public TimeSpan? StartTime { get; set; }

    [Display(Name = "دسته‌بندی")]
    [Required]
    public int? CategoryId { get; set; }

    [Display(Name = "بیوگرافی")]
    [StringLength(200)]
    public string? Bio { get; set; }

    [Display(Name = "نام مستعار")]
    [MaxLength(20)]
    public string? Nickname { get; set; }

    [Display(Name = "رمز عبور")]
    [Required]
    [MinLength(6)]
    public string? Password { get; set; }

    [Display(Name = "تکرار رمز عبور")]
    [Required]
    [Compare(nameof(Password))]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "پذیرش قوانین")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "پذیرش قوانین الزامی است.")]
    public bool AcceptTerms { get; set; }
}
