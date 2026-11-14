using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Diagnostics;
using TechUI.Mvc.Captcha;
using TechUI.Mvc.Chat;
using TechUI.Mvc.Grid;
using TechUI.Mvc.Infrastructure;
using TechUI.SampleMvc.Models;
using TechUI.SampleMvc.Services;
using TechUI.Samples.Shared;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TechUI.SampleMvc.Controllers;

public class HomeController : Controller
{
    private const long MaxUploadSize = 10 * 1024 * 1024;

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".webp"
        };

    private static readonly HashSet<string> FileExtensions =
        new(ImageExtensions, StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".ppt", ".pptx", ".txt", ".csv", ".zip"
        };

    private static readonly FileExtensionContentTypeProvider ContentTypes =
        new();

    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly SupportChatStore _supportChat;
    private readonly DeepSeekAssistant _deepSeek;

    public HomeController(
        ILogger<HomeController> logger,
        IWebHostEnvironment environment,
        SupportChatStore supportChat,
        DeepSeekAssistant deepSeek)
    {
        _logger = logger;
        _environment = environment;
        _supportChat = supportChat;
        _deepSeek = deepSeek;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GridData(TechGridRequest request,
    string? code,
    string? title)
    {
        var allData = Enumerable.Range(1, 1000).Select(i => new UserViewModel
        {
            Id = i,
            FullName = $"User {i:000}",
            IsActive = i % 2 == 0,
            Roles = new List<string>()
            {
                "نقش "+$"User {i:000}",
                "نقش "+$"User {i:000}"+i.ToString()
            }

        })
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(title))
        {
            allData = allData.Where(x =>
                x.FullName.Contains(title));
        }

        return Json(await allData
            .AsQueryable()
            .ToTechGridResultAsync(request, new[]
            {
                nameof(UserViewModel.Id),
                nameof(UserViewModel.FullName),
                nameof(UserViewModel.IsActive)
            }));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GridChat([FromBody] TechGridChatRequest request)
    {
        return Json(GridChatParser.Parse(request));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult DropDownList()
    {
        return View();
    }

    [HttpGet]
    public IActionResult TimePicker()
    {
        return View(new TimeSampleModel
        {
            StartTime = new TimeSpan(9, 30, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TimePicker(TimeSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult Choice()
    {
        return View(new ChoiceSampleModel
        {
            IsActive = true,
            NewsLetter = true,
            Gender = "2"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Choice(ChoiceSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult Editor()
    {
        return View(new EditorSampleModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Editor(EditorSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile? file,
        string? kind,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "فایلی انتخاب نشده است." });
        }

        if (file.Length > MaxUploadSize)
        {
            return BadRequest(new { message = "حجم فایل بیشتر از ۱۰ مگابایت است." });
        }

        var allowed = string.Equals(kind, "image", StringComparison.OrdinalIgnoreCase)
            ? ImageExtensions
            : FileExtensions;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowed.Contains(extension))
        {
            return BadRequest(new { message = "پسوند فایل مجاز نیست." });
        }

        var directory = UploadDirectory;

        Directory.CreateDirectory(directory);

        var name = SafeFileName(file.FileName);
        var target = Path.Combine(directory, name);
        var stem = Path.GetFileNameWithoutExtension(name);
        var suffix = 1;

        while (System.IO.File.Exists(target))
        {
            target = Path.Combine(directory, $"{stem}-{suffix++}{extension}");
        }

        await using (var stream = System.IO.File.Create(target))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var storedName = Path.GetFileName(target);

        _logger.LogInformation(
            "Media uploaded: {File} ({Size} bytes)",
            storedName,
            file.Length);

        return Json(new
        {
            url = Url.Action(nameof(Media), new { name = storedName }),
            name = storedName,
            size = file.Length,
            type = ImageExtensions.Contains(extension) ? "image" : "file"
        });
    }

    [HttpGet]
    public IActionResult FileUpload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public IActionResult FileUpload(List<IFormFile>? documents)
    {
        ViewBag.Submitted = (documents ?? new List<IFormFile>())
            .Where(file => file.Length > 0)
            .Select(file => (file.FileName, file.Length))
            .ToList();

        return View();
    }

    [HttpGet]
    public IActionResult MediaLibrary(string? type)
    {
        var directory = UploadDirectory;

        if (!Directory.Exists(directory))
        {
            return Json(Array.Empty<object>());
        }

        var allowed = string.Equals(type, "image", StringComparison.OrdinalIgnoreCase)
            ? ImageExtensions
            : FileExtensions;

        var items = new DirectoryInfo(directory)
            .GetFiles()
            .Where(f => allowed.Contains(f.Extension))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Select(f => new
            {
                url = Url.Action(nameof(Media), new { name = f.Name }),
                name = f.Name,
                size = f.Length,
                type = ImageExtensions.Contains(f.Extension) ? "image" : "file"
            })
            .ToList();

        return Json(items);
    }

    [HttpGet]
    public IActionResult Media(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            !string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal))
        {
            return NotFound();
        }

        var path = Path.Combine(UploadDirectory, name);

        if (!System.IO.File.Exists(path))
        {
            return NotFound();
        }

        Response.Headers["X-Content-Type-Options"] = "nosniff";

        if (!ContentTypes.TryGetContentType(name, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return PhysicalFile(path, contentType, enableRangeProcessing: true);
    }

    private string UploadDirectory =>
        Path.Combine(_environment.ContentRootPath, "App_Data", "uploads");

    private static string SafeFileName(string name)
    {
        var fileName = Path.GetFileName(name);

        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalid, '-');
        }

        return string.IsNullOrWhiteSpace(fileName) ? "file" : fileName;
    }

    [HttpGet]
    public IActionResult DatePicker()
    {
        return View(new DatePickerSampleModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DatePicker(DatePickerSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult DateRange()
    {
        return View(new DateRangeSampleModel
        {
            FromDate = new DateTime(2026, 10, 1),
            ToDate = new DateTime(2026, 10, 31)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DateRange(DateRangeSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult Captcha()
    {
        return View(new CaptchaSampleModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Captcha(
        CaptchaSampleModel model,
        CancellationToken cancellationToken)
    {
        var captcha = await HttpContext.Request.ValidateTechCaptchaAsync(
            nameof(model.Captcha),
            cancellationToken);

        if (!captcha.IsValid)
        {
            ModelState.AddModelError(
                nameof(model.Captcha),
                captcha.Error ?? "کد امنیتی نامعتبر است.");

            return View(model);
        }

        ModelState.Clear();

        ViewBag.Success = true;

        return View(new CaptchaSampleModel());
    }

    [HttpGet]
    public IActionResult Rtl()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Modal()
    {
        return View(new ModalSampleModel
        {
            ClientCategoryId = 1,
            ServerCategoryId = 2,
            BirthDate = new DateTime(1995, 6, 15),
            StartTime = new TimeSpan(9, 30, 0)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Modal(ModalSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult Inputs()
    {
        return View(new InputSampleModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Inputs(InputSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult Validation()
    {
        return View(new ValidationSampleModel());
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Validation(ValidationSampleModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ViewBag.Submitted = model;

        return View(new ValidationSampleModel());
    }

    [HttpGet]
    public IActionResult MultiSelect()
    {
        return View(new MultiSelectSampleModel
        {
            Roles = new List<string> { "1", "3" },
            CategoryIds = new List<int> { 1, 2 },
            Tags = new List<string> { "csharp", "mvc" }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MultiSelect(MultiSelectSampleModel model)
    {
        ViewBag.Submitted = model;

        return View(model);
    }

    [HttpGet]
    public IActionResult Notifications()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Components()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Charts()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Map()
    {
        return View();
    }

    [HttpGet]
    public IActionResult TreeView()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DropDownTree()
    {
        return View(new DropDownTreeSampleModel());
    }

    [HttpGet]
    public IActionResult Wizard()
    {
        return View(new WizardSampleModel());
    }

    [HttpGet]
    public IActionResult Timeline()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Breadcrumb()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Chat()
    {
        ViewBag.DeepSeekConfigured = _deepSeek.IsConfigured;
        ViewBag.DeepSeekModel = _deepSeek.Model;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChatAssistant(
        [FromBody] TechChatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest();
        }

        var answer = await _deepSeek.ReplyAsync(request, cancellationToken);

        if (answer.Success)
        {
            return Json(new TechChatResponse
            {
                Reply = answer.Reply,
                Sender = $"DeepSeek · {_deepSeek.Model}"
            });
        }

        var reply = LocalAssistant.Reply(request);

        if (_deepSeek.IsConfigured)
        {
            var error = answer.Error ?? "unknown error";

            if (error.Length > 160)
            {
                error = error[..160] + "…";
            }

            reply += "\n\n(اتصال به DeepSeek برقرار نشد: " + error +
                " — پاسخ محلی نمایش داده شد.)";
        }

        return Json(new TechChatResponse
        {
            Reply = reply,
            Sender = _deepSeek.IsConfigured ? "دستیار محلی · خطای DeepSeek" : "دستیار محلی"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChatSupport([FromBody] TechChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
        {
            return BadRequest();
        }

        var messageId = _supportChat.AddUserMessage(
            request.ConversationId ?? string.Empty,
            request.Message.Trim());

        return Json(new TechChatResponse
        {
            Delivered = true,
            MessageId = messageId,
            Typing = true
        });
    }

    [HttpGet]
    public IActionResult ChatSupportPoll(string? conversation, long after)
    {
        return Json(_supportChat.Poll(
            conversation ?? string.Empty, after));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Wizard(WizardSampleModel model)
    {
        if (ModelState.IsValid)
        {
            ViewBag.Submitted = model;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DropDownTree(DropDownTreeSampleModel model)
    {
        if (ModelState.IsValid)
        {
            ViewBag.Submitted = model;
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult GetDropDownList(string? term)
    {
        var temp=new List<ProductViewModel> {
        new ProductViewModel {Id = 1, Text = "کالای دیجیتال",Code="test11",Description="test1" },
        new ProductViewModel{Id = 2, Text = "پوشاک" ,Code="test22",Description="test2"},
        new ProductViewModel{ Id = 3, Text = "کتاب" ,Code="test33",Description="test3"}
        };

        if (term != null) {
           temp= temp.Where(m => m.Text!.Contains(term)).ToList();
        }
        return Json(temp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GetDropDownListPost(string? term)
    {
        return GetDropDownList(term);
    }

    [HttpGet]
    public IActionResult GetCategories()
    {
        return Json(new[]
        {
        new { Id = 1, Text = "دیجیتال" },
        new { Id = 2, Text = "پوشاک" }
    });
    }
    [HttpGet]
    public IActionResult GetSubCategories(int parentId)
    {
        var data = parentId switch
        {
            1 => new[]
            {
            new { Id = 10, Text = "موبایل" },
            new { Id = 11, Text = "لپ‌تاپ" }
        },

            2 => new[]
            {
            new { Id = 20, Text = "مردانه" },
            new { Id = 21, Text = "زنانه" }
        },

            _ => Array.Empty<object>()
        };

        return Json(data);
    }

}
