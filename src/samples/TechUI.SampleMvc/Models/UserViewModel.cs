namespace TechUI.SampleMvc.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public  List<string>? Roles { get; set; }
    }
}
