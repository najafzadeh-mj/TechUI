using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleMvc.Models
{
    public class DateRangeSampleModel
    {
        [Required(ErrorMessage = "انتخاب تاریخ شروع الزامی است.")]
        [Display(Name = "تاریخ شروع")]
        public DateTime? FromDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        public DateTime? ToDate { get; set; }
    }
}
