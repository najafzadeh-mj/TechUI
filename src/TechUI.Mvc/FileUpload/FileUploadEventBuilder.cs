namespace TechUI.Mvc.FileUpload
{
    public sealed class FileUploadEventBuilder
    {
        private readonly FileUploadEventOptions _options;

        internal FileUploadEventBuilder(FileUploadEventOptions options)
        {
            _options = options;
        }

        public FileUploadEventBuilder Change(string handler)
        {
            _options.Change = handler;
            return this;
        }

        public FileUploadEventBuilder Upload(string handler)
        {
            _options.Upload = handler;
            return this;
        }

        public FileUploadEventBuilder Success(string handler)
        {
            _options.Success = handler;
            return this;
        }

        public FileUploadEventBuilder Error(string handler)
        {
            _options.Error = handler;
            return this;
        }

        public FileUploadEventBuilder Complete(string handler)
        {
            _options.Complete = handler;
            return this;
        }

        public FileUploadEventBuilder Remove(string handler)
        {
            _options.Remove = handler;
            return this;
        }
    }
}
