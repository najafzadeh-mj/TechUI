namespace TechUI.SampleMvc.Models;

public class MultiSelectSampleModel
{
    public List<string> Roles { get; set; } = new();

    public List<int> CategoryIds { get; set; } = new();

    public List<string> Tags { get; set; } = new();
}
