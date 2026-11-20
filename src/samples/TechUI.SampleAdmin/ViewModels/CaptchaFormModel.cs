using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleAdmin.ViewModels;

public sealed class CaptchaFormModel
{
    [Display(Name = "کد امنیتی")]
    [Required(ErrorMessage = "کد امنیتی را وارد کنید.")]
    public string? Captcha { get; set; }
}
