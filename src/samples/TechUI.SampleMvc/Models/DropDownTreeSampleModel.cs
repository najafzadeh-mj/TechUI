using System.ComponentModel.DataAnnotations;

namespace TechUI.SampleMvc.Models;

public class DropDownTreeSampleModel
{
    [Display(Name = "دسته‌بندی")]
    [Required(ErrorMessage = "انتخاب دسته‌بندی الزامی است.")]
    public string? Category { get; set; }

    [Display(Name = "حوزه‌های پشتیبانی")]
    public List<string> Areas { get; set; } = new() { "express", "online" };
}
