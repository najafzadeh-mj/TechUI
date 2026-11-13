using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleMvc.Models
{
    public class CaptchaSampleModel
    {
        [Required(ErrorMessage = "کد امنیتی را وارد کنید.")]
        [Display(Name = "کد امنیتی")]
        public string? Captcha { get; set; }
    }
}
