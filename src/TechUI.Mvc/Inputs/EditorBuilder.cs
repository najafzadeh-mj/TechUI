using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Encodings.Web;

namespace TechUI.Mvc.Inputs
{
    public sealed class EditorBuilder : InputBuilderBase<EditorBuilder>
    {
        private static readonly (string Key, string Command, string Icon, string Label)[] AvailableTools =
        {
            ("bold", "bold", "type-bold", "درشت"),
            ("italic", "italic", "type-italic", "کج"),
            ("underline", "underline", "type-underline", "زیرخط"),
            ("ul", "insertUnorderedList", "list-ul", "لیست نقطه‌ای"),
            ("ol", "insertOrderedList", "list-ol", "لیست شماره‌دار"),
            ("link", "createLink", "link-45deg", "افزودن لینک"),
            ("image", "insertImage", "image", "افزودن تصویر"),
            ("file", "insertFile", "paperclip", "افزودن فایل"),
            ("clear", "removeFormat", "eraser", "حذف قالب‌بندی")
        };

        private readonly List<string> _tools =
            AvailableTools.Select(t => t.Key).ToList();

        private int _height = 180;
        private string? _imageUploadUrl;
        private string? _fileUploadUrl;
        private string? _mediaLibraryUrl;
        private string _uploadField = "file";
        private long? _maxUploadSize;

        public EditorBuilder(string name) : base(name)
        {
        }

        public EditorBuilder Height(int pixels)
        {
            _height = pixels > 0 ? pixels : 180;
            return this;
        }

        /// <summary>
        /// Endpoint that accepts a multipart POST and returns
        /// { url, name, size, type } for an uploaded image.
        /// </summary>
        public EditorBuilder ImageUpload(string url)
        {
            _imageUploadUrl = NormalizeUrl(url);
            return this;
        }

        /// <summary>
        /// Endpoint that accepts a multipart POST and returns
        /// { url, name, size, type } for an uploaded file.
        /// </summary>
        public EditorBuilder FileUpload(string url)
        {
            _fileUploadUrl = NormalizeUrl(url);
            return this;
        }

        /// <summary>
        /// Uses the same endpoint for image and file uploads.
        /// </summary>
        public EditorBuilder Upload(string url)
        {
            return ImageUpload(url).FileUpload(url);
        }

        /// <summary>
        /// Endpoint that returns existing media as
        /// [{ url, name, size, type }] for the library picker.
        /// </summary>
        public EditorBuilder MediaLibrary(string url)
        {
            _mediaLibraryUrl = NormalizeUrl(url);
            return this;
        }

        public EditorBuilder UploadField(string fieldName)
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                _uploadField = fieldName.Trim();
            }

            return this;
        }

        public EditorBuilder MaxUploadSize(long bytes)
        {
            _maxUploadSize = bytes > 0 ? bytes : null;
            return this;
        }

        public EditorBuilder Tools(params string[] tools)
        {
            if (tools is null || tools.Length == 0)
            {
                return this;
            }

            var selected = AvailableTools
                .Where(t => tools.Contains(t.Key, StringComparer.OrdinalIgnoreCase))
                .Select(t => t.Key)
                .ToList();

            if (selected.Count > 0)
            {
                _tools.Clear();
                _tools.AddRange(selected);
            }

            return this;
        }

        public override void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            var wrapper = CreateWrapper("editor", "tech-editor");

            var tools = EnabledTools();

            wrapper.Attributes["data-tools"] = string.Join(",", tools);

            if (_imageUploadUrl is not null)
            {
                wrapper.Attributes["data-image-upload"] = _imageUploadUrl;
            }

            if (_fileUploadUrl is not null)
            {
                wrapper.Attributes["data-file-upload"] = _fileUploadUrl;
            }

            if (_mediaLibraryUrl is not null)
            {
                wrapper.Attributes["data-media-library"] = _mediaLibraryUrl;
            }

            if (_imageUploadUrl is not null || _fileUploadUrl is not null)
            {
                wrapper.Attributes["data-upload-field"] = _uploadField;
            }

            if (_maxUploadSize.HasValue)
            {
                wrapper.Attributes["data-max-upload-size"] =
                    _maxUploadSize.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrWhiteSpace(PlaceholderValue))
            {
                wrapper.Attributes["data-placeholder"] = PlaceholderValue;
            }

            if (MaxLengthValue.HasValue)
            {
                wrapper.Attributes["data-maxlength"] =
                    MaxLengthValue.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (IsDisabled)
            {
                wrapper.AddCssClass("tech-editor-disabled");
            }

            if (IsReadOnly || IsDisabled)
            {
                wrapper.Attributes["data-readonly"] = "true";
            }

            var readOnly = IsReadOnly || IsDisabled;

            if (!readOnly && tools.Count > 0)
            {
                wrapper.InnerHtml.AppendHtml(BuildToolbar(tools));
            }

            var body = new TagBuilder("div");

            body.Attributes["id"] = InputName;
            body.Attributes["contenteditable"] = readOnly ? "false" : "true";
            body.Attributes["role"] = "textbox";
            body.Attributes["aria-multiline"] = "true";
            body.Attributes["style"] = $"height:{_height}px";
            body.AddCssClass("tech-editor-body");

            if (!string.IsNullOrWhiteSpace(PlaceholderValue))
            {
                body.Attributes["data-placeholder"] = PlaceholderValue;
                body.Attributes["aria-label"] = PlaceholderValue;
            }

            if (!string.IsNullOrWhiteSpace(DirectionValue))
            {
                body.Attributes["dir"] = DirectionValue;
            }

            if (!string.IsNullOrEmpty(InputValue))
            {
                body.InnerHtml.AppendHtml(InputValue);
            }

            wrapper.InnerHtml.AppendHtml(body);

            var hidden = new TagBuilder("input");

            hidden.Attributes["type"] = "hidden";
            hidden.Attributes["name"] = InputName;
            hidden.Attributes["value"] = InputValue ?? string.Empty;

            ApplyValidationData(hidden);

            if (IsDisabled)
            {
                hidden.Attributes["disabled"] = "disabled";
            }

            wrapper.InnerHtml.AppendHtml(hidden);

            var counter = BuildCounter();

            if (counter is not null)
            {
                wrapper.InnerHtml.AppendHtml(counter);
            }

            writer.Write(Write(wrapper, encoder));
        }

        private List<string> EnabledTools()
        {
            return _tools
                .Where(key => key switch
                {
                    "image" => _imageUploadUrl is not null ||
                        _mediaLibraryUrl is not null,
                    "file" => _fileUploadUrl is not null ||
                        _mediaLibraryUrl is not null,
                    _ => true
                })
                .ToList();
        }

        private static string? NormalizeUrl(string? url)
        {
            return string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        }

        private TagBuilder BuildToolbar(IEnumerable<string> tools)
        {
            var toolbar = new TagBuilder("div");

            toolbar.AddCssClass("tech-editor-toolbar");
            toolbar.Attributes["role"] = "toolbar";

            foreach (var key in tools)
            {
                var tool = AvailableTools.First(t => t.Key == key);

                var button = new TagBuilder("button");

                button.Attributes["type"] = "button";
                button.Attributes["tabindex"] = "-1";
                button.Attributes["data-cmd"] = tool.Command;
                button.Attributes["title"] = tool.Label;
                button.Attributes["aria-label"] = tool.Label;

                button.AddCssClass("tech-editor-tool");

                var icon = new TagBuilder("i");
                icon.AddCssClass($"bi bi-{tool.Icon}");

                button.InnerHtml.AppendHtml(icon);

                toolbar.InnerHtml.AppendHtml(button);
            }

            return toolbar;
        }
    }
}
