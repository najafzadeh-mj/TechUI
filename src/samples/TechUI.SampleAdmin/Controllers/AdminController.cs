using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TechUI.Mvc.Grid;
using TechUI.Mvc.Infrastructure;
using TechUI.SampleAdmin.Data;
using TechUI.SampleAdmin.Models;
using TechUI.SampleAdmin.Services;
using TechUI.SampleAdmin.ViewModels;

namespace TechUI.SampleAdmin.Controllers;

public sealed class AdminController : Controller
{
    private const int LowStockThreshold = 10;

    private static readonly string[] CustomerFields =
    {
        nameof(Customer.Id),
        nameof(Customer.FullName),
        nameof(Customer.Email),
        nameof(Customer.Phone),
        nameof(Customer.City),
        nameof(Customer.IsActive),
        nameof(Customer.Credit),
        nameof(Customer.RegisteredOn),
        nameof(Customer.Tags)
    };

    private static readonly string[] ProductFields =
    {
        nameof(Product.Id),
        nameof(Product.Name),
        nameof(Product.Sku),
        "Category.Name",
        nameof(Product.Price),
        nameof(Product.Stock),
        nameof(Product.IsActive),
        nameof(Product.CreatedOn)
    };

    private static readonly string[] OrderFields =
    {
        nameof(Order.Id),
        nameof(Order.Number),
        "Customer.FullName",
        nameof(Order.OrderDate),
        nameof(Order.Status),
        nameof(Order.Total)
    };

    private readonly AdminDbContext _db;
    private readonly SettingsService _settings;

    public AdminController(AdminDbContext db, SettingsService settings)
    {
        _db = db;
        _settings = settings;
    }

    // ---------------------------------------------------------
    // Dashboard
    // ---------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "داشبورد";
        ViewData["ActiveNav"] = "dashboard";

        var today = DateTime.Today;
        var rangeStart = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

        var orders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= rangeStart)
            .Select(o => new
            {
                o.OrderDate,
                o.Status,
                o.Total,
                City = o.Customer!.City
            })
            .ToListAsync();

        var valid = orders
            .Where(o => o.Status != OrderStatus.Canceled)
            .ToList();

        var monthly = new List<MonthlyPoint>();

        for (var index = 0; index < 6; index++)
        {
            var start = rangeStart.AddMonths(index);
            var end = start.AddMonths(1);
            var slice = valid
                .Where(o => o.OrderDate >= start && o.OrderDate < end)
                .ToList();

            monthly.Add(new MonthlyPoint(
                MonthLabel(start),
                slice.Sum(o => o.Total),
                slice.Count));
        }

        var statusTotal = orders.Count;

        var statuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Paid,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.Canceled
        }
        .Select(status =>
        {
            var count = orders.Count(order => order.Status == status);

            return new StatusSlice(
                status.ToPersian(),
                status.ToBadgeClass(),
                count,
                statusTotal == 0
                    ? 0
                    : (int)Math.Round(count * 100d / statusTotal));
        })
        .ToList();

        var model = new DashboardViewModel
        {
            CustomersCount = await _db.Customers.CountAsync(),
            ActiveCustomersCount = await _db.Customers.CountAsync(c => c.IsActive),
            OrdersCount = await _db.Orders.CountAsync(),
            ProductsCount = await _db.Products.CountAsync(),
            LowStockCount = await _db.Products
                .CountAsync(p => p.IsActive && p.Stock < LowStockThreshold),
            Revenue = valid.Sum(o => o.Total),
            TodayRevenue = valid
                .Where(o => o.OrderDate.Date == today)
                .Sum(o => o.Total),
            AverageOrder = valid.Count == 0
                ? 0
                : valid.Average(o => o.Total),
            Monthly = monthly,
            Statuses = statuses,
            TopCities = valid
                .GroupBy(o => o.City)
                .Select(group => new CitySlice(
                    group.Key,
                    group.Count(),
                    group.Sum(o => o.Total)))
                .OrderByDescending(item => item.Revenue)
                .Take(5)
                .ToList(),
            RecentOrders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(8)
                .ToListAsync(),
            LowStockProducts = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Stock < LowStockThreshold)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }

    // ---------------------------------------------------------
    // Customers
    // ---------------------------------------------------------

    [HttpGet]
    public IActionResult Customers()
    {
        ViewData["Title"] = "مشتریان";
        ViewData["ActiveNav"] = "customers";

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerGrid(
        [FromForm] TechGridRequest request)
    {
        var query = _db.Customers.AsNoTracking();

        return Json(await query.ToTechGridResultAsync(request, CustomerFields));
    }

    [HttpGet]
    public IActionResult CustomerCreate()
    {
        ViewData["Title"] = "مشتری جدید";
        ViewData["ActiveNav"] = "customers";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("مشتریان", "Customers"),
            ("مشتری جدید", null));

        return View("CustomerForm", new CustomerFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerCreate(CustomerFormModel model)
    {
        ViewData["Title"] = "مشتری جدید";
        ViewData["ActiveNav"] = "customers";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("مشتریان", "Customers"),
            ("مشتری جدید", null));

        if (!string.IsNullOrWhiteSpace(model.Email) &&
            await _db.Customers.AnyAsync(c => c.Email == model.Email))
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "این ایمیل قبلاً برای مشتری دیگری ثبت شده است.");
        }

        if (!ModelState.IsValid)
        {
            return View("CustomerForm", model);
        }

        var customer = new Customer();

        Apply(model, customer);

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"مشتری «{customer.FullName}» با موفقیت ثبت شد.";

        return RedirectToAction(nameof(Customers));
    }

    [HttpGet]
    public async Task<IActionResult> CustomerEdit(int id)
    {
        ViewData["Title"] = "ویرایش مشتری";
        ViewData["ActiveNav"] = "customers";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("مشتریان", "Customers"),
            ("ویرایش مشتری", null));

        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return NotFound();
        }

        return View("CustomerForm", ToForm(customer));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerEdit(CustomerFormModel model)
    {
        ViewData["Title"] = "ویرایش مشتری";
        ViewData["ActiveNav"] = "customers";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("مشتریان", "Customers"),
            ("ویرایش مشتری", null));

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == model.Id);

        if (customer is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(model.Email) &&
            await _db.Customers.AnyAsync(c =>
                c.Email == model.Email && c.Id != model.Id))
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "این ایمیل قبلاً برای مشتری دیگری ثبت شده است.");
        }

        if (!ModelState.IsValid)
        {
            return View("CustomerForm", model);
        }

        Apply(model, customer);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"تغییرات مشتری «{customer.FullName}» ذخیره شد.";

        return RedirectToAction(nameof(Customers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomerDelete(int id)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null)
        {
            return Json(new { success = false, message = "مشتری یافت نشد." });
        }

        if (await _db.Orders.AnyAsync(o => o.CustomerId == id))
        {
            return Json(new
            {
                success = false,
                message = "این مشتری سفارش ثبت‌شده دارد و قابل حذف نیست."
            });
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = $"مشتری «{customer.FullName}» حذف شد."
        });
    }

    // ---------------------------------------------------------
    // Products
    // ---------------------------------------------------------

    [HttpGet]
    public IActionResult Products()
    {
        ViewData["Title"] = "محصولات";
        ViewData["ActiveNav"] = "products";

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductGrid(
        [FromForm] TechGridRequest request)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category);

        return Json(await query.ToTechGridResultAsync(request, ProductFields));
    }

    [HttpGet]
    public IActionResult ProductCreate()
    {
        ViewData["Title"] = "محصول جدید";
        ViewData["ActiveNav"] = "products";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("محصولات", "Products"),
            ("محصول جدید", null));

        return View("ProductForm", new ProductFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCreate(ProductFormModel model)
    {
        ViewData["Title"] = "محصول جدید";
        ViewData["ActiveNav"] = "products";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("محصولات", "Products"),
            ("محصول جدید", null));

        if (model.CategoryId.HasValue &&
            !await _db.Categories.AnyAsync(c => c.Id == model.CategoryId))
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "دسته‌بندی انتخاب‌شده معتبر نیست.");
        }

        if (!string.IsNullOrWhiteSpace(model.Sku) &&
            await _db.Products.AnyAsync(p => p.Sku == model.Sku))
        {
            ModelState.AddModelError(
                nameof(model.Sku),
                "این کد کالا قبلاً ثبت شده است.");
        }

        if (!ModelState.IsValid)
        {
            return View("ProductForm", model);
        }

        var product = new Product();

        Apply(model, product);

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"محصول «{product.Name}» با موفقیت ثبت شد.";

        return RedirectToAction(nameof(Products));
    }

    [HttpGet]
    public async Task<IActionResult> ProductEdit(int id)
    {
        ViewData["Title"] = "ویرایش محصول";
        ViewData["ActiveNav"] = "products";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("محصولات", "Products"),
            ("ویرایش محصول", null));

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return View("ProductForm", ToForm(product));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductEdit(ProductFormModel model)
    {
        ViewData["Title"] = "ویرایش محصول";
        ViewData["ActiveNav"] = "products";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("محصولات", "Products"),
            ("ویرایش محصول", null));

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == model.Id);

        if (product is null)
        {
            return NotFound();
        }

        if (model.CategoryId.HasValue &&
            !await _db.Categories.AnyAsync(c => c.Id == model.CategoryId))
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "دسته‌بندی انتخاب‌شده معتبر نیست.");
        }

        if (!string.IsNullOrWhiteSpace(model.Sku) &&
            await _db.Products.AnyAsync(p =>
                p.Sku == model.Sku && p.Id != model.Id))
        {
            ModelState.AddModelError(
                nameof(model.Sku),
                "این کد کالا قبلاً ثبت شده است.");
        }

        if (!ModelState.IsValid)
        {
            return View("ProductForm", model);
        }

        Apply(model, product);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"تغییرات محصول «{product.Name}» ذخیره شد.";

        return RedirectToAction(nameof(Products));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductDelete(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return Json(new { success = false, message = "محصول یافت نشد." });
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = $"محصول «{product.Name}» حذف شد."
        });
    }

    // ---------------------------------------------------------
    // Orders
    // ---------------------------------------------------------

    [HttpGet]
    public IActionResult Orders()
    {
        ViewData["Title"] = "سفارش‌ها";
        ViewData["ActiveNav"] = "orders";

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderGrid(
        [FromForm] TechGridRequest request)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer);

        return Json(await query.ToTechGridResultAsync(request, OrderFields));
    }

    [HttpGet]
    public async Task<IActionResult> OrderCreate()
    {
        ViewData["Title"] = "سفارش جدید";
        ViewData["ActiveNav"] = "orders";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("سفارش‌ها", "Orders"),
            ("سفارش جدید", null));
        ViewBag.OrderNumber = await NextOrderNumberAsync();

        return View("OrderForm", new OrderFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderCreate(OrderFormModel model)
    {
        ViewData["Title"] = "سفارش جدید";
        ViewData["ActiveNav"] = "orders";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("سفارش‌ها", "Orders"),
            ("سفارش جدید", null));
        ViewBag.OrderNumber = await NextOrderNumberAsync();

        await ValidateOrderCustomerAsync(model);

        if (!ModelState.IsValid)
        {
            return View("OrderForm", model);
        }

        var order = new Order
        {
            Number = ViewBag.OrderNumber as string ?? await NextOrderNumberAsync()
        };

        Apply(model, order);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"سفارش «{order.Number}» با موفقیت ثبت شد.";

        return RedirectToAction(nameof(Orders));
    }

    [HttpGet]
    public async Task<IActionResult> OrderEdit(int id)
    {
        ViewData["Title"] = "ویرایش سفارش";
        ViewData["ActiveNav"] = "orders";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("سفارش‌ها", "Orders"),
            ("ویرایش سفارش", null));

        var order = await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        return View("OrderForm", ToForm(order));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderEdit(OrderFormModel model)
    {
        ViewData["Title"] = "ویرایش سفارش";
        ViewData["ActiveNav"] = "orders";
        SetBreadcrumb(
            ("پنل", "Index"),
            ("سفارش‌ها", "Orders"),
            ("ویرایش سفارش", null));

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == model.Id);

        if (order is null)
        {
            return NotFound();
        }

        model.Number = order.Number;

        await ValidateOrderCustomerAsync(model);

        if (!ModelState.IsValid)
        {
            return View("OrderForm", model);
        }

        Apply(model, order);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"تغییرات سفارش «{order.Number}» ذخیره شد.";

        return RedirectToAction(nameof(Orders));
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id)
    {
        ViewData["Title"] = "جزئیات سفارش";
        ViewData["ActiveNav"] = "orders";

        var order = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        SetBreadcrumb(
            ("پنل", "Index"),
            ("سفارش‌ها", "Orders"),
            ($"سفارش {order.Number}", null));

        ViewBag.CustomerOrders = await _db.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == order.CustomerId && o.Id != order.Id)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OrderDelete(int id)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Json(new { success = false, message = "سفارش یافت نشد." });
        }

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = $"سفارش «{order.Number}» حذف شد."
        });
    }

    // ---------------------------------------------------------
    // Lookups
    // ---------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> CustomerLookup(string? term)
    {
        var query = _db.Customers
            .AsNoTracking()
            .Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(c => c.FullName.Contains(term));
        }

        var items = await query
            .OrderBy(c => c.FullName)
            .Take(50)
            .Select(c => new
            {
                value = c.Id,
                text = c.FullName + " — " + c.City
            })
            .ToListAsync();

        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> CategoryLookup()
    {
        var items = await _db.Categories
            .AsNoTracking()
            .Include(c => c.Parent)
            .Where(c => c.ParentId != null)
            .OrderBy(c => c.Parent!.Name)
            .ThenBy(c => c.Name)
            .Select(c => new
            {
                value = c.Id,
                text = c.Parent!.Name + " / " + c.Name
            })
            .ToListAsync();

        return Json(items);
    }

    // ---------------------------------------------------------
    // Reports
    // ---------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Reports(DateTime? from, DateTime? to)
    {
        ViewData["Title"] = "گزارش فروش";
        ViewData["ActiveNav"] = "reports";

        return View(await BuildReportAsync(from, to));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReportGrid(
        [FromForm] TechGridRequest request,
        DateTime? from,
        DateTime? to)
    {
        var (start, end) = NormalizeRange(from, to);

        var query = _db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Where(o => o.OrderDate >= start && o.OrderDate < end.AddDays(1));

        return Json(await query.ToTechGridResultAsync(request, OrderFields));
    }

    // ---------------------------------------------------------
    // Settings
    // ---------------------------------------------------------

    [HttpGet]
    public IActionResult Settings()
    {
        ViewData["Title"] = "تنظیمات";
        ViewData["ActiveNav"] = "settings";

        return View(_settings.Load());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Settings(AdminSettings model)
    {
        ViewData["Title"] = "تنظیمات";
        ViewData["ActiveNav"] = "settings";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _settings.Save(model);

        TempData["Success"] = "تنظیمات با موفقیت ذخیره شد.";

        return RedirectToAction(nameof(Settings));
    }

    // ---------------------------------------------------------
    // Error
    // ---------------------------------------------------------

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewData["Title"] = "خطا";
        ViewData["ActiveNav"] = string.Empty;

        return View();
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    private void SetBreadcrumb(params (string Text, string? Action)[] trail)
    {
        ViewData["Breadcrumb"] = trail.ToList();
    }

    private static string MonthLabel(DateTime date)
    {
        return date.ToString("MMMM", CultureInfo.GetCultureInfo("fa-IR"));
    }

    private async Task<ReportsViewModel> BuildReportAsync(
        DateTime? from,
        DateTime? to)
    {
        var (start, end) = NormalizeRange(from, to);

        var rows = await _db.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= start && o.OrderDate < end.AddDays(1))
            .Select(o => new { o.Status, o.Total })
            .ToListAsync();

        var valid = rows
            .Where(row => row.Status != OrderStatus.Canceled)
            .ToList();

        var total = rows.Count;

        var statuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Paid,
            OrderStatus.Shipped,
            OrderStatus.Delivered,
            OrderStatus.Canceled
        }
        .Select(status =>
        {
            var count = rows.Count(row => row.Status == status);

            return new StatusSlice(
                status.ToPersian(),
                status.ToBadgeClass(),
                count,
                total == 0 ? 0 : (int)Math.Round(count * 100d / total));
        })
        .ToList();

        return new ReportsViewModel
        {
            FromDate = start,
            ToDate = end,
            TotalOrders = total,
            DeliveredCount = rows.Count(row => row.Status == OrderStatus.Delivered),
            CanceledCount = rows.Count(row => row.Status == OrderStatus.Canceled),
            TotalRevenue = valid.Sum(row => row.Total),
            AverageOrder = valid.Count == 0 ? 0 : valid.Average(row => row.Total),
            Statuses = statuses
        };
    }

    private static (DateTime Start, DateTime End) NormalizeRange(
        DateTime? from,
        DateTime? to)
    {
        var end = (to ?? DateTime.Today).Date;
        var start = (from ?? end.AddDays(-29)).Date;

        if (start > end)
        {
            (start, end) = (end, start);
        }

        return (start, end);
    }

    private async Task<string> NextOrderNumberAsync()
    {
        var numbers = await _db.Orders
            .AsNoTracking()
            .Select(o => o.Number)
            .ToListAsync();

        var last = numbers
            .Select(number => int.TryParse(
                number[(number.LastIndexOf('-') + 1)..],
                out var value)
                    ? value
                    : 0)
            .DefaultIfEmpty(1000)
            .Max();

        return $"ORD-{last + 1}";
    }

    private async Task ValidateOrderCustomerAsync(OrderFormModel model)
    {
        if (model.CustomerId.HasValue &&
            !await _db.Customers.AnyAsync(c => c.Id == model.CustomerId))
        {
            ModelState.AddModelError(
                nameof(model.CustomerId),
                "مشتری انتخاب‌شده معتبر نیست.");
        }
    }

    private static CustomerFormModel ToForm(Customer customer)
    {
        return new CustomerFormModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            City = customer.City,
            Credit = customer.Credit,
            RegisteredOn = customer.RegisteredOn,
            Tags = string.IsNullOrWhiteSpace(customer.Tags)
                ? new List<string>()
                : customer.Tags
                    .Split(',', StringSplitOptions.RemoveEmptyEntries |
                                StringSplitOptions.TrimEntries)
                    .ToList(),
            Notes = customer.Notes,
            IsActive = customer.IsActive
        };
    }

    private static void Apply(CustomerFormModel model, Customer customer)
    {
        customer.FullName = model.FullName.Trim();
        customer.Email = model.Email.Trim();
        customer.Phone = string.IsNullOrWhiteSpace(model.Phone)
            ? null
            : model.Phone.Trim();
        customer.City = model.City;
        customer.Credit = model.Credit;
        customer.RegisteredOn = model.RegisteredOn;
        customer.Tags = model.Tags.Count == 0
            ? null
            : string.Join(",", model.Tags);
        customer.Notes = string.IsNullOrWhiteSpace(model.Notes)
            ? null
            : model.Notes.Trim();
        customer.IsActive = model.IsActive;
    }

    private static ProductFormModel ToForm(Product product)
    {
        return new ProductFormModel
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            CategoryId = product.CategoryId,
            Price = product.Price,
            Stock = product.Stock,
            CreatedOn = product.CreatedOn,
            IsActive = product.IsActive,
            Description = product.Description
        };
    }

    private static void Apply(ProductFormModel model, Product product)
    {
        product.Name = model.Name.Trim();
        product.Sku = model.Sku.Trim();
        product.CategoryId = model.CategoryId!.Value;
        product.Price = model.Price;
        product.Stock = model.Stock;
        product.CreatedOn = model.CreatedOn;
        product.IsActive = model.IsActive;
        product.Description = string.IsNullOrWhiteSpace(model.Description)
            ? null
            : model.Description.Trim();
    }

    private static OrderFormModel ToForm(Order order)
    {
        return new OrderFormModel
        {
            Id = order.Id,
            Number = order.Number,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Total = order.Total,
            Note = order.Note
        };
    }

    private static void Apply(OrderFormModel model, Order order)
    {
        order.CustomerId = model.CustomerId!.Value;
        order.OrderDate = model.OrderDate;
        order.Status = model.Status;
        order.Total = model.Total;
        order.Note = string.IsNullOrWhiteSpace(model.Note)
            ? null
            : model.Note.Trim();
    }
}
