namespace TechUI.SampleMvc.Models
{
    public class ProductViewModel
    {
        public int? Id { get; set; }
        public string? Text { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }

    public class CategoryViewModel
    {
        public int? CategoryId { get; set; }
        public string? Text { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
    public class SubCategorytViewModel
    {
        public int? SubCategoryId { get; set; }
        public string? Text { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}
