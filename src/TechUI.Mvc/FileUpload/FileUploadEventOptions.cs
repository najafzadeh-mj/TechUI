namespace TechUI.Mvc.FileUpload
{
    public sealed class FileUploadEventOptions
    {
        public string? Change { get; set; }
        public string? Upload { get; set; }
        public string? Success { get; set; }
        public string? Error { get; set; }
        public string? Complete { get; set; }
        public string? Remove { get; set; }
    }
}
