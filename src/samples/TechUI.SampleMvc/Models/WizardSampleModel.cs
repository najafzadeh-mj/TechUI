using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleMvc.Models;

public class WizardSampleModel
{
    [Display(Name = "نام و نام خانوادگی")]
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
    public string? FullName { get; set; }

    [Display(Name = "ایمیل")]
    [Required(ErrorMessage = "ایمیل الزامی است.")]
    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    public string? Email { get; set; }

    [Display(Name = "شرکت")]
    [Required(ErrorMessage = "نام شرکت الزامی است.")]
    public string? Company { get; set; }

    [Display(Name = "نوع حساب")]
    [Required(ErrorMessage = "نوع حساب را انتخاب کنید.")]
    public string? Plan { get; set; }

    [Display(Name = "توضیحات")]
    [StringLength(300, ErrorMessage = "توضیحات حداکثر ۳۰۰ کاراکتر است.")]
    public string? Notes { get; set; }
}
