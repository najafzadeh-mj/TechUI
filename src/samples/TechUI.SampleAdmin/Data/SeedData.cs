using TechUI.SampleAdmin.Models;

namespace TechUI.SampleAdmin.Data;

public static class SeedData
{
    private static readonly string[] FirstNames =
    {
        "علی", "رضا", "محمد", "حسین", "مهدی", "سارا", "مریم",
        "زهرا", "فاطمه", "نگار", "امیر", "پارسا", "نیما", "سینا",
        "آرش", "الهام", "شیما", "بهنام", "کیان", "یاسمن"
    };

    private static readonly string[] LastNames =
    {
        "رضایی", "محمدی", "حسینی", "کریمی", "موسوی", "احمدی",
        "جعفری", "صادقی", "نوری", "عباسی", "قاسمی", "رحیمی",
        "شریفی", "کاظمی", "مرادی", "سلطانی", "زمانی", "اکبری",
        "فرهادی", "بهرامی"
    };

    private static readonly string[] Cities =
    {
        "تهران", "مشهد", "اصفهان", "شیراز", "تبریز", "کرج", "اهواز", "قم"
    };

    private static readonly string[] Tags =
    {
        "وفادار", "عمده‌فروش", "خرده‌فروش", "سازمانی", "مشتری جدید"
    };

    public static void Initialize(AdminDbContext db)
    {
        db.Database.EnsureCreated();

        if (db.Customers.Any())
        {
            return;
        }

        var random = new Random(2026);

        var categories = SeedCategories(db);
        SeedProducts(db, categories, random);
        var customers = SeedCustomers(db, random);
        SeedOrders(db, customers, random);
    }

    private static Dictionary<string, Category> SeedCategories(AdminDbContext db)
    {
        var digital = new Category { Name = "کالای دیجیتال" };
        var fashion = new Category { Name = "مد و پوشاک" };
        var home = new Category { Name = "خانه و آشپزخانه" };

        db.Categories.AddRange(digital, fashion, home);
        db.SaveChanges();

        var groups = new Dictionary<string, Category>
        {
            ["digital"] = digital,
            ["fashion"] = fashion,
            ["home"] = home
        };

        var children = new (string Key, string Name, Category Parent)[]
        {
            ("mobile", "موبایل و تبلت", digital),
            ("laptop", "لپ‌تاپ و کامپیوتر", digital),
            ("audio", "هدفون و صدا", digital),
            ("wearable", "ساعت هوشمند", digital),
            ("men", "پوشاک مردانه", fashion),
            ("women", "پوشاک زنانه", fashion),
            ("shoes", "کفش", fashion),
            ("accessories", "اکسسوری", fashion),
            ("appliances", "لوازم برقی", home),
            ("kitchen", "ظروف و سرو", home),
            ("decor", "دکوراسیون", home)
        };

        foreach (var (key, name, parent) in children)
        {
            var category = new Category { Name = name, Parent = parent };
            db.Categories.Add(category);
            groups[key] = category;
        }

        db.SaveChanges();

        return groups;
    }

    private static void SeedProducts(
        AdminDbContext db,
        Dictionary<string, Category> groups,
        Random random)
    {
        var catalog = new (string Category, string Name, decimal Price, int Stock)[]
        {
            ("mobile", "گوشی سامسونگ گلکسی S24", 58_900_000, 24),
            ("mobile", "گوشی شیائومی Redmi Note 13", 12_500_000, 68),
            ("mobile", "گوشی آیفون ۱۵", 92_000_000, 12),
            ("mobile", "تبلت لنوو Tab M11", 18_900_000, 31),
            ("laptop", "لپ‌تاپ ایسوس Vivobook 16", 46_000_000, 9),
            ("laptop", "لپ‌تاپ لنوو ThinkPad E14", 52_000_000, 7),
            ("laptop", "کیبورد مکانیکی ردراگون", 2_850_000, 45),
            ("laptop", "ماوس لاجیتک MX Master 3S", 5_600_000, 28),
            ("audio", "هدفون سونی WH-1000XM5", 18_700_000, 16),
            ("audio", "ایرپاد اپل AirPods Pro 2", 12_900_000, 22),
            ("audio", "اسپیکر JBL Flip 6", 5_200_000, 39),
            ("wearable", "ساعت هوشمند اپل Watch SE", 21_500_000, 14),
            ("wearable", "ساعت هوشمند سامسونگ Watch 6", 14_800_000, 19),
            ("men", "تی‌شرت نخی مردانه", 690_000, 120),
            ("men", "پیراهن آستین بلند", 1_250_000, 84),
            ("men", "شلوار جین مردانه", 1_890_000, 56),
            ("women", "مانتو کتان تابستانی", 2_450_000, 42),
            ("women", "شال ابریشمی", 780_000, 96),
            ("women", "لباس مجلسی زنانه", 4_900_000, 23),
            ("shoes", "کفش ورزشی نایک Air", 4_650_000, 33),
            ("shoes", "کفش چرم مردانه", 3_800_000, 27),
            ("shoes", "صندل زنانه", 1_200_000, 61),
            ("accessories", "کیف چرم دستی", 2_900_000, 18),
            ("accessories", "عینک آفتابی پلاریزه", 1_650_000, 37),
            ("accessories", "ساعت مچی کلاسیک", 3_400_000, 15),
            ("appliances", "چای‌ساز برقی", 2_300_000, 48),
            ("appliances", "جاروبرقی رباتیک", 15_600_000, 11),
            ("appliances", "مایکروویو ۲۵ لیتری", 9_800_000, 8),
            ("kitchen", "سرویس قابلمه استیل", 4_200_000, 26),
            ("kitchen", "ست چاقو سرامیکی", 1_450_000, 54),
            ("decor", "گلدان سرامیکی", 420_000, 73),
            ("decor", "تابلو دیوارکوب", 980_000, 41),
            ("decor", "فرش ماشینی ۶ متری", 7_800_000, 13)
        };

        var index = 1;

        foreach (var (category, name, price, stock) in catalog)
        {
            db.Products.Add(new Product
            {
                Name = name,
                Sku = $"SKU-{index:0000}",
                CategoryId = groups[category].Id,
                Price = price,
                Stock = stock,
                IsActive = index % 9 != 0,
                CreatedOn = DateTime.Today.AddDays(-random.Next(30, 500)),
                Description = $"{name} با کیفیت بالا و گارانتی معتبر فروشگاه."
            });

            index++;
        }

        db.SaveChanges();
    }

    private static List<Customer> SeedCustomers(AdminDbContext db, Random random)
    {
        var customers = new List<Customer>();

        for (var i = 0; i < 42; i++)
        {
            var first = FirstNames[i % FirstNames.Length];
            var last = LastNames[(i * 7) % LastNames.Length];
            var city = Cities[random.Next(Cities.Length)];

            var itemTags = new List<string>();

            if (i % 3 == 0)
            {
                itemTags.Add(Tags[0]);
            }

            if (i % 5 == 0)
            {
                itemTags.Add(Tags[1]);
            }

            if (i % 4 == 0)
            {
                itemTags.Add(Tags[3]);
            }

            if (itemTags.Count == 0)
            {
                itemTags.Add(Tags[random.Next(Tags.Length)]);
            }

            customers.Add(new Customer
            {
                FullName = $"{first} {last}",
                Email = $"customer{i + 1:00}@techui.sample",
                Phone = $"0912{random.Next(1000000, 9999999)}",
                City = city,
                IsActive = i % 7 != 0,
                Credit = random.Next(0, 21) * 1_000_000m,
                RegisteredOn = DateTime.Today.AddDays(-random.Next(30, 730)),
                Tags = string.Join(",", itemTags),
                Notes = i % 6 == 0 ? "مشتری از طریق کمپین تابستانه جذب شده است." : null
            });
        }

        db.Customers.AddRange(customers);
        db.SaveChanges();

        return customers;
    }

    private static void SeedOrders(
        AdminDbContext db,
        List<Customer> customers,
        Random random)
    {
        var today = DateTime.Today;

        for (var i = 1; i <= 240; i++)
        {
            var roll = random.Next(100);

            var status = roll switch
            {
                < 6 => OrderStatus.Canceled,
                < 16 => OrderStatus.Pending,
                < 42 => OrderStatus.Paid,
                < 65 => OrderStatus.Shipped,
                _ => OrderStatus.Delivered
            };

            var date = today
                .AddDays(-random.Next(0, 240))
                .AddHours(random.Next(8, 22))
                .AddMinutes(random.Next(0, 60));

            db.Orders.Add(new Order
            {
                Number = $"ORD-{1000 + i}",
                CustomerId = customers[random.Next(customers.Count)].Id,
                OrderDate = date,
                Status = status,
                Total = random.Next(80, 4500) * 10_000m,
                Note = random.Next(100) < 18
                    ? "ارسال با پست پیشتاز درخواست شده است."
                    : null
            });
        }

        db.SaveChanges();
    }
}
