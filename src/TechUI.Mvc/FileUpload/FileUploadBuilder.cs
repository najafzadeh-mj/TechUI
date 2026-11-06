using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;
using TechUI.Mvc.Infrastructure;

namespace TechUI.Mvc.FileUpload
{
    /// <summary>
    /// Drop-zone file picker with client validation, optional AJAX upload
    /// and native form-post fallback.
    /// </summary>
    public sealed class FileUploadBuilder : IHtmlContent
    {
        private readonly IHtmlHelper _html;
        private readonly List<string> _accept = new();
        private readonly FileUploadEventOptions _events = new();
        private readonly Dictionary<string, string> _attributes =
            new(StringComparer.OrdinalIgnoreCase);

        private string _name;
        private string? _id;
        private string? _url;
        private string _field = "file";
        private string? _title;
        private string? _hint;
        private string? _browseText;
        private bool _multiple;
        private bool _autoUpload = true;
        private bool _thumbnails = true;
        private bool _showFileList = true;
        private bool _dragDrop = true;
        private bool _disabled;
        private bool _required;
        private long _maxSize;
        private int _maxFiles;

        internal FileUploadBuilder(IHtmlHelper html, string name)
        {
            _html = html;
            _name = name;
        }

        public FileUploadBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public FileUploadBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public FileUploadBuilder Multiple(bool multiple = true)
        {
            _multiple = multiple;
            return this;
        }

        public FileUploadBuilder Accept(string accept)
        {
            if (!string.IsNullOrWhiteSpace(accept))
            {
                _accept.AddRange(accept.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries));
            }

            return this;
        }

        public FileUploadBuilder Accept(params string[] accept)
        {
            foreach (var value in accept ?? Array.Empty<string>())
            {
                Accept(value);
            }

            return this;
        }

        public FileUploadBuilder MaxSize(long bytes)
        {
            _maxSize = bytes > 0 ? bytes : 0;
            return this;
        }

        public FileUploadBuilder MaxSizeMb(int megabytes)
        {
            return MaxSize(megabytes > 0 ? megabytes * 1024L * 1024L : 0);
        }

        public FileUploadBuilder MaxFiles(int maxFiles)
        {
            _maxFiles = maxFiles > 0 ? maxFiles : 0;
            return this;
        }

        public FileUploadBuilder Url(string url)
        {
            _url = url;
            return this;
        }

        public FileUploadBuilder Field(string field)
        {
            if (!string.IsNullOrWhiteSpace(field))
            {
                _field = field;
            }

            return this;
        }

        public FileUploadBuilder AutoUpload(bool auto = true)
        {
            _autoUpload = auto;
            return this;
        }

        public FileUploadBuilder Thumbnails(bool enabled = true)
        {
            _thumbnails = enabled;
            return this;
        }

        public FileUploadBuilder ShowFileList(bool show = true)
        {
            _showFileList = show;
            return this;
        }

        public FileUploadBuilder DragDrop(bool enabled = true)
        {
            _dragDrop = enabled;
            return this;
        }

        public FileUploadBuilder Disabled(bool disabled = true)
        {
            _disabled = disabled;
            return this;
        }

        public FileUploadBuilder Required(bool required = true)
        {
            _required = required;
            return this;
        }

        public FileUploadBuilder Title(string text)
        {
            _title = text;
            return this;
        }

        public FileUploadBuilder Hint(string text)
        {
            _hint = text;
            return this;
        }

        public FileUploadBuilder BrowseText(string text)
        {
            _browseText = text;
            return this;
        }

        public FileUploadBuilder Events(Action<FileUploadEventBuilder> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var builder = new FileUploadEventBuilder(_events);
            action(builder);

            return this;
        }

        public FileUploadBuilder Attribute(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _attributes[name] = value;
            }

            return this;
        }

        public FileUploadBuilder Attributes(
            IEnumerable<KeyValuePair<string, string>> attributes)
        {
            foreach (var attribute in attributes)
            {
                Attribute(attribute.Key, attribute.Value);
            }

            return this;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var id = string.IsNullOrWhiteSpace(_id) ? _name : _id;
            var inputId = id + "-input";
            var accept = string.Join(",", _accept);

            var root = new TagBuilder("div");

            root.AddCssClass("tech-input tech-upload");
            root.Attributes["id"] = id;
            root.Attributes["data-tech-component"] = "fileupload";
            root.Attributes["data-name"] = _name;
            root.Attributes["data-multiple"] = Bool(_multiple);
            root.Attributes["data-auto-upload"] = Bool(_autoUpload);
            root.Attributes["data-thumbnails"] = Bool(_thumbnails);
            root.Attributes["data-file-list"] = Bool(_showFileList);
            root.Attributes["data-drag-drop"] = Bool(_dragDrop);
            root.Attributes["data-required"] = Bool(_required);
            root.Attributes["data-disabled"] = Bool(_disabled);

            if (!string.IsNullOrWhiteSpace(accept))
            {
                root.Attributes["data-accept"] = accept;
            }

            if (_maxSize > 0)
            {
                root.Attributes["data-max-size"] =
                    _maxSize.ToString(CultureInfo.InvariantCulture);
            }

            if (_maxFiles > 0)
            {
                root.Attributes["data-max-files"] =
                    _maxFiles.ToString(CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(_url))
            {
                root.Attributes["data-url"] = _url;
                root.Attributes["data-field"] = _field;

                var token = TechAntiforgery.GetToken(_html);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    root.Attributes["data-antiforgery"] = token;
                }
            }

            if (_disabled)
            {
                root.AddCssClass("tech-upload-disabled");
            }

            if (!string.IsNullOrWhiteSpace(_events.Change))
            {
                root.Attributes["data-event-change"] = _events.Change;
            }

            if (!string.IsNullOrWhiteSpace(_events.Upload))
            {
                root.Attributes["data-event-upload"] = _events.Upload;
            }

            if (!string.IsNullOrWhiteSpace(_events.Success))
            {
                root.Attributes["data-event-success"] = _events.Success;
            }

            if (!string.IsNullOrWhiteSpace(_events.Error))
            {
                root.Attributes["data-event-error"] = _events.Error;
            }

            if (!string.IsNullOrWhiteSpace(_events.Complete))
            {
                root.Attributes["data-event-complete"] = _events.Complete;
            }

            if (!string.IsNullOrWhiteSpace(_events.Remove))
            {
                root.Attributes["data-event-remove"] = _events.Remove;
            }

            foreach (var attribute in _attributes)
            {
                if (attribute.Key.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    root.AddCssClass(attribute.Value);
                }
                else if (!root.Attributes.ContainsKey(attribute.Key))
                {
                    root.Attributes[attribute.Key] = attribute.Value;
                }
            }

            root.InnerHtml.AppendHtml(BuildZone(
                id,
                inputId,
                accept,
                _hint ?? BuildHint(accept)));

            if (_showFileList)
            {
                var list = new TagBuilder("div");

                list.AddCssClass("tech-upload-list");
                list.Attributes["role"] = "list";
                list.Attributes["aria-live"] = "polite";

                root.InnerHtml.AppendHtml(list);
            }

            var error = new TagBuilder("div");

            error.AddCssClass("tech-upload-error");
            error.Attributes["role"] = "alert";
            error.Attributes["hidden"] = "hidden";

            root.InnerHtml.AppendHtml(error);

            var values = new TagBuilder("div");

            values.AddCssClass("tech-upload-values");

            root.InnerHtml.AppendHtml(values);

            root.WriteTo(writer, encoder);
        }

        private TagBuilder BuildZone(
            string id,
            string inputId,
            string accept,
            string? hint)
        {
            var zoneId = id + "-zone";
            var zone = new TagBuilder("label");

            zone.AddCssClass("tech-upload-zone");
            zone.Attributes["for"] = inputId;
            zone.Attributes["id"] = zoneId;

            if (_disabled)
            {
                zone.Attributes["aria-disabled"] = "true";
            }

            var icon = new TagBuilder("span");

            icon.AddCssClass("tech-upload-icon");
            icon.Attributes["aria-hidden"] = "true";

            var iconGlyph = new TagBuilder("i");

            iconGlyph.AddCssClass("bi bi-cloud-arrow-up");
            icon.InnerHtml.AppendHtml(iconGlyph);

            zone.InnerHtml.AppendHtml(icon);

            var title = new TagBuilder("span");

            title.AddCssClass("tech-upload-title");
            title.InnerHtml.Append(_title ?? (_multiple
                ? "فایل‌ها را اینجا رها کنید یا برای انتخاب کلیک کنید"
                : "فایل را اینجا رها کنید یا برای انتخاب کلیک کنید"));

            zone.InnerHtml.AppendHtml(title);

            var hintId = id + "-hint";
            var hintElement = new TagBuilder("span");

            hintElement.AddCssClass("tech-upload-hint");
            hintElement.Attributes["id"] = hintId;

            if (string.IsNullOrWhiteSpace(hint))
            {
                hintElement.Attributes["hidden"] = "hidden";
            }
            else
            {
                hintElement.InnerHtml.Append(hint);
            }

            zone.InnerHtml.AppendHtml(hintElement);

            var browse = new TagBuilder("span");

            browse.AddCssClass("tech-upload-browse");
            browse.InnerHtml.Append(_browseText ?? "انتخاب فایل");

            zone.InnerHtml.AppendHtml(browse);

            var input = new TagBuilder("input");

            input.Attributes["type"] = "file";
            input.Attributes["id"] = inputId;
            input.Attributes["name"] = _name;
            input.AddCssClass("tech-upload-input");
            input.Attributes["aria-describedby"] = hintId;

            if (_multiple)
            {
                input.Attributes["multiple"] = "multiple";
            }

            if (!string.IsNullOrWhiteSpace(accept))
            {
                input.Attributes["accept"] = accept;
            }

            if (_disabled)
            {
                input.Attributes["disabled"] = "disabled";
            }

            if (_required && string.IsNullOrWhiteSpace(_url))
            {
                input.Attributes["required"] = "required";
                input.Attributes["aria-required"] = "true";
            }

            zone.InnerHtml.AppendHtml(input);

            return zone;
        }

        private string? BuildHint(string accept)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(accept))
            {
                parts.Add("فرمت‌های مجاز: " + FormatAccept(accept));
            }

            if (_maxSize > 0)
            {
                parts.Add("حداکثر " + FormatSize(_maxSize));
            }

            if (_maxFiles > 0)
            {
                parts.Add("حداکثر " + _maxFiles.ToString(
                    CultureInfo.InvariantCulture) + " فایل");
            }

            return parts.Count == 0 ? null : string.Join(" — ", parts);
        }

        private static string FormatAccept(string accept)
        {
            return string.Join(
                "، ",
                accept
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .ToArray());
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024L)
            {
                return (bytes / (1024d * 1024d))
                    .ToString("0.##", CultureInfo.InvariantCulture) + " MB";
            }

            if (bytes >= 1024L)
            {
                return (bytes / 1024d)
                    .ToString("0.##", CultureInfo.InvariantCulture) + " KB";
            }

            return bytes.ToString(CultureInfo.InvariantCulture) + " B";
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
