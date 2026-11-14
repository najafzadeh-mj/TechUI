# مرجع کامپوننت‌های TechUI.Mvc

این سند API سمت سرور (Fluent) و API سمت کلاینت (JavaScript) همه‌ی کامپوننت‌ها را
پوشش می‌دهد. نمونه‌ی اجرایی هر کامپوننت در
`src/samples/TechUI.SampleMvc/Views/Home` قرار دارد و نمونه‌ی کاربردی کامل
(پنل مدیریت با EF Core و SQLite) در `src/samples/TechUI.SampleAdmin` است.

- [راه‌اندازی](#راهاندازی)
- [قواعد مشترک ورودی‌ها](#قواعد-مشترک-ورودیها)
- [Grid](#grid)
- [DropDownList](#dropdownlist)
- [ورودی‌های متنی](#ورودیهای-متنی)
- [CheckBox و RadioButton](#checkbox-و-radiobutton)
- [DatePicker](#datepicker)
- [DateRangePicker](#daterangepicker)
- [TimePicker](#timepicker)
- [MultiSelect و Tag Input](#multiselect-و-tag-input)
- [Tabs](#tabs)
- [Accordion](#accordion)
- [TreeView](#treeview)
- [DropDownTree](#dropdowntree)
- [FileUpload](#fileupload)
- [Captcha](#captcha)
- [Wizard / Stepper](#wizard--stepper)
- [Timeline](#timeline)
- [Breadcrumb](#breadcrumb)
- [Chat](#chat)
- [Chart](#chart)
- [Map](#map)
- [Spinner](#spinner)
- [Toast / Notification](#toast--notification)
- [Dialog / Confirm / Alert](#dialog--confirm--alert)
- [اعتبارسنجی فرم](#اعتبارسنجی-فرم)
- [RTL](#rtl)
- [تم تاریک](#تم-تاریک)
- [قالب‌های رنگ](#قالبهای-رنگ)
- [Template Helpers در Grid](#template-helpers-در-grid)
- [رویدادها](#رویدادها)
- [یکپارچگی EF Core](#یکپارچگی-ef-core)

## راه‌اندازی

پیش‌نیاز ظاهری: **Bootstrap 5.3 (CSS)** برای فرم‌ها، دکمه‌ها و badge‌ها و
**Bootstrap Icons** برای آیکن‌ها. هر دو جزو دارایی‌های peer هستند و همراه بسته
نصب نمی‌شوند. TechUI به جاوااسکریپت Bootstrap نیاز ندارد؛ فایل
`bootstrap.bundle.min.js` را فقط در صورت استفاده از کامپوننت‌های تعاملی خود
Bootstrap بارگذاری کنید.

در `_ViewImports.cshtml`:

```razor
@using TechUI.Mvc.Html
@using TechUI.Mvc.Infrastructure
```

در `_Layout.cshtml` (Bootstrap CSS قبل از `techui.css` بارگذاری شود تا
override‌های کامپوننت‌ها اعمال شوند):

```html
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/_content/TechUI.Mvc/css/techui.css" />
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
...
<script src="~/_content/TechUI.Mvc/js/techui.js"></script>
```

همه‌ی کامپوننت‌ها با اسکن `data-tech-component` روی `DOMContentLoaded` فعال
می‌شوند. اگر HTML را بعد از لود صفحه تزریق می‌کنید، `Tech.boot()` را صدا بزنید.

---

## قواعد مشترک ورودی‌ها

همه‌ی builder‌های ورودی (`TextBox`, `TextArea`, `NumericTextBox`,
`MaskedTextBox`, `Editor`, `CheckBox`, `RadioButton`, `DatePicker`,
`DateRangePicker`, `TimePicker`) از `InputBuilderBase<TBuilder>` ارث می‌برند و
این متدها را دارند:

| متد | توضیح |
| --- | --- |
| `Name(string)` | نام و `id` کنترل |
| `Value(object?)` | مقدار اولیه |
| `Placeholder(string)` | متن راهنما |
| `CssClass(string)` | افزودن کلاس CSS |
| `Disabled(bool = true)` | غیرفعال |
| `ReadOnly(bool = true)` | فقط‌خواندنی |
| `Required(bool = true)` | افزودن `required` |
| `MaxLength(int)` | حداکثر طول |
| `ShowCounter(bool = true)` | شمارنده‌ی کاراکتر |
| `PrefixIcon(string)` / `SuffixIcon(string)` | آیکون Bootstrap Icons بدون پیشوند `bi-` |
| `Direction("rtl" \| "ltr")` | جهت کنترل |
| `Change(string handler)` | نام تابع سراسری برای رویداد `Change` |
| `Attribute(name, value)` / `Attributes(...)` | Attribute دلخواه |
| `Data(name, value)` | `data-*` دلخواه |
| `ValidationAttributes(...)` / `ValidationError(string?)` / `ValidationMessage(bool = true)` | کنترل دستی اعتبارسنجی |

متدهای `*For<TModel, TValue>(...)` مقدار مدل، `ModelState` و
DataAnnotations را به‌صورت خودکار اعمال می‌کنند.

---

## Grid

### ساخت سمت سرور

```razor
@(Html.Tech().Grid<UserViewModel>()
    .Name("usersGrid")
    .Height(600)
    .Pageable()
    .Sortable()
    .Sort("Id", "desc")
    .Filterable()
    .MultiSelect()
    .Resizable()
    .Reorderable()
    .AriaLabel("جدول کاربران")
    .PageSize(20)
    .PageSizeSelector()
    .PageSizes(10, 20, 50, 100)
    .DataSource(ds => ds
        .Read("GridData", "Home")          // Url(action, controller, area?)
        .Type(TechHttpMethod.Post)
        .Data("searchGrid"))               // تابع JS برای پارامترهای اضافی
    .Events(e => e
        .DataBinding("onGridBinding")
        .DataBound("onGridBound")
        .Change("onGridChange")
        .RowClick("onGridRowClick")
        .RowDoubleClick("onGridRowDoubleClick")
        .Error("onGridError"))
    .Toolbar(t =>
    {
        t.Create("افزودن", "onCreate", "plus");
        t.Refresh("بازخوانی");
        t.Export("خروجی CSV", "filetype-csv", "users.csv");
        t.ColumnChooser();
    })
    .Summary(s => s
        .Count()
        .Average(x => x.Id, "میانگین شناسه"))
    .Group(g => g
        .By(x => x.IsActive, "وضعیت فعالیت")
        .By(x => x.FullName, "نام کامل"))
    .GroupPanel()
    .Columns(c =>
    {
        c.Bound(x => x.Id).Title("شناسه").Width(100).Filterable().Pinned();
        c.Bound(x => x.FullName).Title("نام کامل").Sortable(false);
        c.Template("<a href='/Users/Edit/{{Id}}'>{{FullName}}</a>")
            .Title("عملیات")
            .Width(140);
    }))
```

| متد | توضیح |
| --- | --- |
| `Name(string)` | `id` گرید و کلید `Tech.Grid.get` |
| `Height(int)` | ارتفاع بدنه (پیکسل) |
| `Pageable`, `Sortable`, `Filterable`, `Selectable`, `Resizable`, `Reorderable` | فعال‌سازی قابلیت‌ها |
| `MultiSelect(bool = true)` | ستون چک‌باکس برای انتخاب چند ردیف؛ `Selectable` را هم فعال می‌کند |
| `Sort(string field, string dir = "asc")` | مرتب‌سازی اولیه؛ چندبار فراخوانی = چندستونه |
| `AriaLabel(string)` | نام دسترس‌پذیر جدول (روی ظرف و `<table>`) |
| `PageSize(int)` | اندازه‌ی صفحه‌ی اولیه |
| `PageSizeSelector()` / `PageSizes(params int[])` | انتخابگر اندازه‌ی صفحه؛ `PageSizes` انتخابگر را خودکار فعال می‌کند |
| `InfiniteScroll(bool = true)` | جایگزینی صفحه‌بند با بارگذاری تدریجی؛ `PageSize` اندازه‌ی هر بخش است |
| `DataSource(Action<GridDataSourceBuilder>)` | `Read(action, controller, area?)`, `Url(url)`, `Data(functionName)`, `Type(TechHttpMethod)` |
| `Events(Action<GridEventBuilder>)` | `DataBinding`, `DataBound`, `Change`, `RowClick`, `RowDoubleClick`, `GroupToggle`, `Error` |
| `Toolbar(Action<GridToolbarBuilder>)` | `Create(text?, handler?, icon?)`, `Refresh(text?, icon?)`, `ColumnChooser(text?, icon?)`, `Export(text?, icon?, fileName?)`, `Custom(text, handler, icon?)` |
| `Chat(Action<GridChatBuilder>)` | پنل چت دستیار؛ `Url(url)`, `Title`, `Placeholder`, `Welcome`, `ButtonText`, `Icon`, `MaxLength`, `Suggestions(...)` |
| `Summary(Action<GridSummaryBuilder<TModel>>)` | ردیف خلاصه در `tfoot`؛ `Count(field?, label?)`, `Sum/Average/Min/Max(expression, label?)` |
| `Group(Action<GridGroupBuilder<TModel>>)` | گروه‌بندی سمت سرور روی نتیجه‌ی کامل؛ `By(expression, title?)` یا `By(field, title?)`؛ چندبار فراخوانی = گروه‌های تودرتو؛ اگر سرور پشتیبانی نکند، مرورگر صفحه‌ی جاری را گروه‌بندی می‌کند |
| `GroupPanel(bool = true)` | دکمه و پنل «گروه‌بندی» در نوار ابزار تا کاربر فیلدها را اضافه/حذف/جابه‌جا کند؛ انتخاب کاربر در `localStorage` می‌ماند |
| `Columns(Action<GridColumnFactory<TModel>>)` | `Bound(expression)` و `Template(string)` |

`GridColumnBuilder`: `Title`, `Width`, `Sortable`, `Filterable`, `Pinned(bool = true)` و
`Field(string)`. `Field` برای ستون‌های `Template` است تا بتوانند در
مرتب‌سازی/فیلتر و خلاصه‌ها با نام یک فیلد واقعی شرکت کنند.

ستون‌های `Bound` نوع داده را هم برای کلاینت ارسال می‌کنند؛ پاپ‌آپ فیلتر برای
ستون رشته‌ای به‌صورت پیش‌فرض `contains` و برای ستون عددی، تاریخی یا بولین
`eq` را انتخاب می‌کند و فقط عملگرهای سازگار را نشان می‌دهد. برای ستون
بولین به‌جای ورودی متن، انتخابگر «بله / خیر» نمایش داده می‌شود. `Bound` از
عبارت‌های تودرتو مانند `x => x.Customer.Name` هم پشتیبانی می‌کند و مسیر
`Customer.Name` را برای فیلتر/مرتب‌سازی سمت سرور می‌فرستد.

### قرارداد کنترلر

```csharp
[HttpPost]
public async Task<IActionResult> GridData(TechGridRequest request)
{
    var result = await _db.Users
        .ToTechGridResultAsync(request, new[] { "Id", "FullName", "IsActive" });

    return Json(result);
}
```

`TechGridResult<T>` شامل `Data`, `Total`, `Page`, `PageSize`, `Summary` و
در حالت گروه‌بندی `Grouped` و `RowTotal` است. فیلترها در `Filter` (یک گروه
با `Logic` برابر `and`/`or` و لیست `Filters` شامل `Field`, `Operator`,
`Value`)، مرتب‌سازی‌ها در `Sorts` (`Field`, `Dir`)، سطوح گروه‌بندی در
`Groups` (`Field`, `Title`) و درخواست خلاصه‌ها در `Summary` (`Field`,
`Type`) قرار می‌گیرند. عملگرهای پشتیبانی‌شده:
`contains`, `notcontains`, `startswith`, `endswith`, `eq`, `neq`, `gt`, `gte`,
`lt`, `lte`, `in`, `notin`, `isnull`, `isnotnull`. انواع خلاصه:
`count`, `sum`, `average`, `min`, `max`.

### API سمت کلاینت

```js
const grid = Tech.Grid.get('usersGrid');

await grid.refresh();
grid.selectedData();                       // ردیف انتخاب‌شده یا اولین ردیف انتخاب چندگانه
grid.selectedRows();                       // آرایه‌ی ردیف‌های انتخاب‌شده‌ی صفحه‌ی جاری
grid.getData();                            // ردیف‌های صفحه‌ی جاری
grid.clearSelection();                     // پاک کردن انتخاب تکی و چندگانه
grid.exportCsv('users.csv');               // خروجی CSV از صفحه‌ی جاری
grid.setFilter('FullName', 'علی');         // فقط state را تغییر می‌دهد
grid.clearFilters();
await grid.refresh();                      // برای اعمال فیلترها
grid.setGroups([{ field: 'Customer.FullName', title: 'مشتری' }]);
grid.addGroup('Status', 'وضعیت');          // افزودن یک سطح جدید
grid.removeGroup('Status');
grid.clearGroups();                        // حذف کامل گروه‌بندی
grid.getGroups();                          // [{ field, title }]
grid.expandAllGroups();                    // باز کردن همه‌ی گروه‌ها
grid.collapseAllGroups();                  // بستن همه‌ی گروه‌ها
grid.getCollapsedGroups();                 // کلید گروه‌های بسته
grid.hasMore();                            // در حالت اسکرول بی‌نهایت: بخش دیگری مانده؟
await grid.loadMore();                     // بارگذاری دستی بخش بعد
grid.state();                              // { page, pageSize, sorts, filters, groups, collapsedGroups, highlight }
```

`setFilter` و `clearFilters` درخواستی ارسال نمی‌کنند؛ بعد از آن‌ها
`refresh()` را صدا بزنید. عملگر پیش‌فرض `setFilter` بر اساس نوع ستون انتخاب
می‌شود و در صورت نیاز می‌توانید عملگر را به‌صورت آرگومان سوم بدهید.

مرتب‌سازی چندستونه با نگه‌داشتن `Shift` و کلیک روی سرستون‌ها انجام می‌شود.
ستون‌های مخفی‌شده، عرض و ترتیب ستون‌ها و اندازه‌ی صفحه در `localStorage` ذخیره
می‌شوند. سرستون‌های قابل مرتب‌سازی با صفحه‌کلید (`Enter` / `Space`) هم کار
می‌کنند.

### انتخاب چندگانه

با `MultiSelect()` یک ستون چک‌باکس به ابتدای جدول اضافه می‌شود. چک‌باکس
سرستون همه‌ی ردیف‌های صفحه‌ی جاری را انتخاب/لغو می‌کند و در انتخاب جزئی،
حالت `indeterminate` می‌گیرد. کلیک روی ردیف یا `Space` روی سلول هم انتخاب
آن ردیف را تغییر می‌دهد. `selectedRows()` ردیف‌های انتخاب‌شده را به ترتیب
صفحه برمی‌گرداند، `selectedData()` اولین آن‌ها را می‌دهد و `clearSelection()`
همه را پاک می‌کند. انتخاب با بارگذاری صفحه‌ی جدید بازنشانی می‌شود. payload
رخداد `Change` در این حالت علاوه بر `data`، فیلد `selected` (آرایه‌ی
ردیف‌ها) را هم دارد.

### خروجی CSV

`Export(text?, icon?, fileName?)` دکمه‌ای به نوار ابزار اضافه می‌کند که
صفحه‌ی جاری را به‌صورت CSV دانلود می‌کند. فقط ستون‌های نمایان خروجی
می‌گیرند؛ برای ستون‌های `Template` متن رندرشده‌ی سلول و برای ستون‌های
`Bound` مقدار نمایش‌داده‌شده نوشته می‌شود. فایل با UTF-8 BOM ذخیره می‌شود
تا فارسی در Excel درست باز شود و مقادیر دارای کاما/گیومه/خط جدید طبق
استاندارد CSV escape می‌شوند. همین کار با `grid.exportCsv(fileName)` هم
قابل انجام است. در حالت گروه‌بندی، فقط ردیف‌های نمایان (گروه‌های باز) در
خروجی می‌آیند؛ برای خروجی کامل ابتدا `expandAllGroups()` را صدا بزنید.

### ستون‌های سنجاق‌شده

با `Pinned()` ستون در لبه‌ی شروع جدول ثابت می‌ماند و هنگام اسکرول افقی روی
بقیه‌ی ستون‌ها می‌نشیند. رفتار با RTL سازگار است: در حالت راست‌به‌چپ ستون
از سمت راست و در LTR از سمت چپ ثابت می‌شود. عرض ستون‌ها پس از هر رندر،
تغییر عرض ستون و تغییر اندازه‌ی پنجره دوباره محاسبه می‌شود و سایه‌ی مرز فقط
روی آخرین ستون سنجاق‌شده می‌افتد.

> ستون‌های سنجاق‌شده را از ابتدای جدول انتخاب کنید؛ سنجاق‌کردن ستونی که
> ستون‌های غیرسنجاق‌شده قبل از آن هستند باعث می‌شود آن ستون‌ها هنگام اسکرول
> زیر ستون سنجاق‌شده پنهان شوند. اگر `MultiSelect()` فعال و حداقل یک ستون
> سنجاق‌شده موجود باشد، ستون چک‌باکس هم خودکار سنجاق می‌شود.

### ردیف خلاصه

`Summary(...)` یک ردیف `tfoot` زیر داده‌ها اضافه می‌کند. `ToTechGridResultAsync`
مقادیر را روی **همه‌ی ردیف‌های فیلترشده** (نه فقط صفحه‌ی جاری) محاسبه
می‌کند، درخواست را در `Summary` می‌خواند و نتیجه را در
`TechGridResult.Summary` برمی‌گرداند.

```razor
.Summary(s => s
    .Count(label: "تعداد سفارش")
    .Sum(x => x.Total)
    .Average(x => x.Total, "میانگین"))
```

ستون‌های `Bound` به‌طور خودکار فیلد دارند؛ برای ستون‌های `Template` نام فیلد
واقعی را با `Field("Total")` مشخص کنید تا خلاصه و مرتب‌سازی/فیلتر روی آن
کار کند و ستون خلاصه زیر همان ستون رندر شود.

اگر endpoint سفارشی مقداری در `Summary` برنگرداند، کلاینت خلاصه را از
ردیف‌های صفحه‌ی جاری محاسبه می‌کند؛ در این حالت `sum`/`average` فقط همان
صفحه را پوشش می‌دهند. برچسب پیش‌فرض هر نوع: تعداد، جمع، میانگین، کمینه،
بیشینه. payload رخداد `DataBound` فیلد `summary` را هم دارد.

### گروه‌بندی ردیف‌ها

`Group(...)` گروه‌بندی را بر اساس یک یا چند فیلد تعریف می‌کند. اگر
endpoint با `ToTechGridResultAsync` ساخته شده باشد، گروه‌بندی روی
**کل نتیجه‌ی فیلترشده** در سرور انجام می‌شود و صفحه‌بندی روی گروه‌های
سطح اول اعمال می‌گردد. هر فراخوانی `By` یک سطح تودرتو اضافه می‌کند:

```razor
.Group(g => g
    .By(x => x.Customer!.FullName, "مشتری")
    .By(x => x.Status, "وضعیت"))
```

`By` هم عبارت (مانند `Bound`) و هم نام فیلد رشته‌ای را می‌پذیرد و
مسیرهای تودرتو مثل `Customer.FullName` را پشتیبانی می‌کند. اگر عنوان
داده نشود، عنوان ستون هم‌نام استفاده می‌شود. مقادیر بولین به «بله/خیر» و
مقدار خالی به «(خالی)» تبدیل می‌شوند؛ ردیف هر گروه تعداد ردیف‌های همان
گروه را نشان می‌دهد.

هر سرگروه یک دکمه‌ی باز/بسته دارد که با صفحه‌کلید هم کار می‌کند و
`aria-expanded` را به‌روز می‌کند. وضعیت بسته‌بودن در `Set` کلاینت نگه
داشته می‌شود و با بارگذاری مجدد صفحه حفظ می‌شود:

```js
const grid = Tech.Grid.get('ordersGrid');

grid.collapseAllGroups();
grid.expandAllGroups();
grid.getCollapsedGroups();   // ["Customer.FullName=...","..."]
```

رخداد `GroupToggle` با `Events(e => e.GroupToggle("onGroupToggle"))` ثبت
می‌شود و payload آن `{ sender, key, collapsed }` است؛ `key` همان کلید
گروه (فیلد + مقدار، با پیشوند کلید والد برای گروه‌های تودرتو) است.

#### گروه‌بندی سمت سرور

کلاینت سطوح فعال را در درخواست با `Groups[0].Field` و به‌صورت اختیاری
`Groups[0].Title` می‌فرستد. `ToTechGridResultAsync` این فیلدها را با
همان `allowedFields` و قواعد مسیرهای تودرتو (`Customer.FullName`) که برای
مرتب‌سازی و فیلتر استفاده می‌شود اعتبارسنجی می‌کند و فقط برای انواع
ساده — عدد، رشته، `bool`، `enum`، `Guid`، `decimal`، تاریخ/زمان و مشابه
آن — گروه می‌سازد.

قرارداد نتیجه در حالت گروه‌بندی‌شده:

| فیلد | معنی |
| --- | --- |
| `Data` | ردیف‌های همه‌ی گروه‌های سطح اولِ همان صفحه (به ترتیب گروه و سپس `Sorts`) |
| `Total` | تعداد **گروه‌های سطح اول** (نه ردیف‌ها) |
| `RowTotal` | تعداد کل ردیف‌های فیلترشده |
| `Grouped` | `true` وقتی گروه‌بندی و صفحه‌بندی گروهی در سرور انجام شده باشد |
| `PageSize` | تعداد گروه‌های سطح اول در هر صفحه؛ تعداد صفحات = `Total / PageSize` |

`Summary` همچنان روی همه‌ی ردیف‌های فیلترشده محاسبه می‌شود. کلاینت در این
حالت برچسب صفحه‌بندی را «تعداد گروه:» و شمارنده را
«(N گروه، M رکورد)» نشان می‌دهد و `aria-rowcount` را با `RowTotal` پر
می‌کند. اگر گروه‌بندی جلوتر از مرتب‌سازی، در هر سطح، ردیف‌ها بر اساس مقدار
فیلد گروه صعودی و سپس `Sort`های کاربر مرتب می‌شوند.

> گروه‌بندی سمت سرور «همه یا هیچ» است: اگر حتی یک سطح مجاز نباشد، قابل
> تبدیل به عبارت LINQ نباشد یا نوعش ساده نباشد (مثلاً `List<string>`)،
> سرور کل گروه‌بندی را نادیده می‌گیرد، `Grouped` را `false` برمی‌گرداند
> و صفحه‌بندی عادی ردیف‌ها انجام می‌شود؛ در این حالت مرورگر فقط صفحه‌ی
> جاری را گروه‌بندی می‌کند. گروه‌بندی روی فیلدی با تعداد مقادیر یکتا و
> زیاد، تعداد گروه‌های صفحه را بالا می‌برد؛ صفحه‌بندی همچنان معیار
> «تعداد گروه در صفحه» است، بنابراین ممکن است ردیف‌های یک صفحه زیاد شوند.

#### تغییر گروه‌بندی توسط کاربر

با `.GroupPanel()` دکمه‌ی «گروه‌بندی» به نوار ابزار اضافه می‌شود. پنل آن
سطوح فعال را با شماره نشان می‌دهد و کاربر می‌تواند سطح جدید از بین
ستون‌های دارای فیلد اضافه کند، سطح‌ها را بالا/پایین ببرد، یک سطح را حذف
کند یا کل گروه‌بندی را بردارد. پنل با `Escape`، کلیک بیرون، اسکرول یا
تغییر اندازه‌ی پنجره بسته می‌شود و فوکوس را به دکمه برمی‌گرداند.

```razor
.Group(g => g.By(x => x.Customer!.FullName, "مشتری"))
.GroupPanel()
```

انتخاب کاربر (حتی حذف کامل گروه‌بندی) در `localStorage` با کلید
`tech-grid-groups-<id>` ذخیره می‌شود و بر تنظیم `Group(...)` سمت سرور
اولویت دارد؛ برای بازگشت به پیش‌فرض سرور، این کلید را پاک کنید. با
تغییر گروه‌بندی، وضعیت بسته‌بودن گروه‌ها پاک می‌شود. همان کارها با
`setGroups`, `addGroup`, `removeGroup`, `clearGroups` و `getGroups` هم
قابل انجام است. تنها ستون‌هایی که فیلد دارند در پنل پیشنهاد می‌شوند.

> اگر گروه‌بندی سمت سرور پشتیبانی نشود، گروه‌بندی فقط روی **صفحه‌ی جاری**
> اعمال می‌شود. خروجی CSV نیز در هر دو حالت فقط ردیف‌های نمایان (گروه‌های
> باز) را شامل می‌شود؛ برای خروجی کامل ابتدا `expandAllGroups()` را صدا
> بزنید.

### بارگذاری بی‌نهایت

با `.InfiniteScroll()` صفحه‌بند با بارگذاری تدریجی جایگزین می‌شود: هنگام
رسیدن بدنه‌ی جدول به انتهای خود (یا پنجره در حالت بدون `Height`)، بخش
بعدی از سرور گرفته و به ردیف‌های قبلی اضافه می‌شود. قرارداد سرور تغییر
نمی‌کند و همان `Page`/`PageSize` است؛ `PageSize` اندازه‌ی هر بخش است.

```razor
.PageSize(20)
.InfiniteScroll()
```

زیر جدول نوار وضعیتی با متن «نمایش X از Y رکورد» و دکمه‌ی «بارگذاری
بیشتر» نمایش داده می‌شود؛ دکمه برای صفحه‌کلید و وقتی اسکرول ممکن نیست
(مثلاً بدنه کوتاه است) کاربرد دارد. پس از پایان داده‌ها پیام «همه‌ی
ردیف‌ها نمایش داده شد.» می‌آید و `aria-rowcount` همچنان تعداد کل
رکوردهای فیلترشده را نشان می‌دهد. متن وضعیت داخل نوار `role="status"` و
`aria-live="polite"` دارد تا تعداد بارگذاری‌شده برای صفحه‌خوان اعلام شود؛
دکمه بیرون از این ناحیه است تا تغییر وضعیت دکمه باعث خواندن دوباره نشود.

- تغییر فیلتر، مرتب‌سازی، گروه‌بندی یا `refresh()` بارگذاری را از بخش اول
  شروع می‌کند و ردیف‌های قبلی را دور می‌ریزد.
- در گروه‌بندی سمت سرور، هر بخش گروه‌های بعدی را اضافه می‌کند. اگر سرور
  گروه‌بندی را پشتیبانی نکند (گروه‌بندی کلاینتی)، بعد از هر بارگذاری همه‌ی
  ردیف‌های بارگذاری‌شده دوباره گروه‌بندی می‌شوند تا سرگروه تکراری ساخته
  نشود.
- انتخاب چندگانه، بستن گروه‌ها، مرتب‌سازی ستون‌ها و ناوبری کیبورد بعد از
  بارگذاری بخش‌های جدید هم کار می‌کنند.
- `exportCsv` همه‌ی ردیف‌های بارگذاری‌شده را خروجی می‌دهد، نه فقط آخرین
  بخش.
- انتخابگر اندازه‌ی صفحه در این حالت نمایش داده نمی‌شود؛ اندازه‌ی بخش را با
  `PageSize` یا `setPageSize` تعیین کنید.
- این حالت ردیف‌های بارگذاری‌شده را در DOM نگه می‌دارد (مجازی‌سازی سطرها
  انجام نمی‌شود)؛ برای مجموعه‌های بسیار بزرگ صفحه‌بندی معمولی مناسب‌تر است.

```js
const grid = Tech.Grid.get('usersGrid');

grid.hasMore();              // آیا بخش دیگری مانده؟
await grid.loadMore();       // بارگذاری دستی بخش بعد
```

### ناوبری کیبورد سلولی

سلول‌های داده `tabindex` چرخشی دارند: `Tab` وارد جدول می‌شود و سپس
`ArrowUp`/`ArrowDown` بین ردیف‌ها، `ArrowLeft`/`ArrowRight` بین ستون‌ها
(با توجه به جهت RTL)، `Home`/`End` ابتدا/انتهای ردیف، `Ctrl+Home`/
`Ctrl+End` ابتدا/انتهای جدول و `PageUp`/`PageDown` ده ردیف جابه‌جا می‌شوند.
فوکوس پس از رندر مجدد (مثلاً تغییر عرض یا مخفی‌کردن ستون) روی همان سلول
باقی می‌ماند و فوکوس داخل لینک‌ها/دکمه‌ها/ورودی‌های سلول با کلیدهای جهت
مداخله نمی‌کند.

فیلترهای فعال در نواری بالای جدول به شکل چیپ نمایش داده می‌شوند؛ هر چیپ با
دکمه‌ی × حذف می‌شود و «پاک کردن همه» تمام فیلترها را یک‌جا برمی‌دارد. این
نوار با تغییر state توسط API یا دستیار گرید هم به‌روز می‌شود.

با `Reorderable()`، دستگیره‌ای کنار هر سرستون ظاهر می‌شود که با کشیدن آن
ستون جابه‌جا و ترتیب جدید ذخیره می‌شود. کلیک/مرتب‌سازی و تغییر عرض با
دستگیره‌ی resize تداخل ندارند. رندر مجدد، موقعیت اسکرول افقی و عمودی را
حفظ می‌کند.

`RowClick` و `RowDoubleClick` با payload
`{ sender, index, row, data }` ارسال می‌شوند و کلیک روی لینک‌ها، دکمه‌ها و
ورودی‌های داخل ردیف نادیده گرفته می‌شود.

دسترسی‌پذیری: سرستون‌ها `aria-sort` (`none`/`ascending`/`descending`)،
صفحه‌ی جاری `aria-current="page"`، جدول `aria-label`/`aria-rowcount`/
`aria-colcount` و ظرف گرید حین بارگذاری `aria-busy` را منتشر می‌کنند.
چک‌باکس‌های انتخاب همه/ردیف برچسب `aria-label` دارند و ردیف‌های انتخاب‌شده
`aria-selected` می‌گیرند.

عنصر گرید هم `element.techGrid` را در اختیار می‌گذارد. رخدادها با `sender`
(نمونه‌ی گرید) ارسال می‌شوند؛ `DataBound` مقدار `data` را هم دارد.

### چت دستیار (Grid Chat)

مانند `AddGridChatTools` در Telerik، یک دکمه و پنل چت داخل نوار ابزار گرید
قرار می‌گیرد؛ اما TechUI هیچ وابستگی به سرویس یا SDK هوش مصنوعی ندارد.
هر پیام به یک endpoint دلخواه شما ارسال می‌شود و پاسخ، متن و مجموعه‌ای از
دستورهای امن برای تغییر state گرید است.

```razor
@(Html.Tech().Grid<Customer>()
    .Name("customersGrid")
    .DataSource(ds => ds.Read("CustomerGrid", "Admin").Type(TechHttpMethod.Post))
    .Pageable().Sortable().Filterable()
    .Chat(c => c
        .Url(Url.Action("GridChat", "Admin")!)
        .Title("دستیار مشتریان")
        .ButtonText("دستیار")
        .Placeholder("مثلاً: شهر تهران را فیلتر کن")
        .Welcome("سلام! دربارهٔ این جدول بپرسید.")
        .Suggestions(
            "فیلتر شهر تهران",
            "مرتب‌سازی بر اساس نام صعودی",
            "صفحه بعد"))
    .Columns(c => { /* ... */ }))
```

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Url(string)` | — | آدرس endpoint چت (الزامی) |
| `Title(string)` | «دستیار گرید» | عنوان پنل |
| `Placeholder(string)` | «دستور خود را بنویسید…» | متن راهنمای ورودی |
| `Welcome(string)` | — | پیام خوش‌آمد قبل از اولین دستور |
| `ButtonText(string)` | «دستیار» | متن دکمهٔ نوار ابزار |
| `Icon(string)` | `chat-dots` | آیکون Bootstrap Icons |
| `MaxLength(int)` | `500` | حداکثر طول پیام (۱ تا ۴۰۰۰) |
| `Suggestions(params string[])` | — | دستورهای پیشنهادی به‌صورت چیپ |

اگر گرید با `GET` داده می‌خواند و چت فعال است، توکن ضدجعل همچنان روی ریشهٔ
گرید رندر می‌شود؛ کلاینت آن را در هدر `RequestVerificationToken` می‌فرستد.
endpoint را با `[HttpPost]` و `[ValidateAntiForgeryToken]` محافظت کنید:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult GridChat([FromBody] TechGridChatRequest request)
{
    return Json(GridChatParser.Parse(request));   // یا فراخوانی مدل/سرویس شما
}
```

#### قرارداد درخواست

```json
{
  "message": "شهر تهران را فیلتر کن",
  "history": [
    { "role": "user", "content": "..." },
    { "role": "assistant", "content": "..." }
  ],
  "grid": {
    "id": "customersGrid",
    "total": 42,
    "columns": [
      {
        "field": "City",
        "title": "شهر",
        "dataType": "string",
        "sortable": true,
        "filterable": true,
        "hidden": false
      }
    ],
    "state": {
      "page": 1,
      "pageSize": 20,
      "sorts": [{ "field": "FullName", "dir": "asc" }],
      "filter": {
        "logic": "and",
        "filters": [{ "field": "City", "operator": "contains", "value": "تهران" }]
      }
    }
  }
}
```

مدل‌های آماده: `TechGridChatRequest`, `TechGridChatMessage`,
`TechGridChatGrid`, `TechGridChatColumn`, `TechGridChatState`. `history`
حداکثر ۱۲ نوبت اخیر را دارد. اگر endpoint شما به مدل زبانی وصل است،
`TechGridChatTools.BuildSystemPrompt(request.Grid, "مشتریان")` توضیح ستون‌ها
و قرارداد خروجی را به‌صورت آماده می‌سازد؛ پاسخ مدل را به
`TechGridChatResponse` نگاشت و برگردانید. `AllowedOperators` و
`AllowedActionTypes` هم برای اعتبارسنجی در دسترس‌اند.

#### قرارداد پاسخ

```json
{
  "reply": "ستون «شهر» شامل «تهران» فیلتر شد.",
  "actions": [
    { "type": "filter", "field": "City", "operator": "contains", "value": "تهران" }
  ]
}
```

| `type` | پارامترها | رفتار |
| --- | --- | --- |
| `filter` | `field`, `operator`, `value` | جایگزینی فیلتر همان ستون، بازگشت به صفحهٔ ۱ و بارگذاری مجدد |
| `clearFilters` | — | حذف همهٔ فیلترها و بارگذاری مجدد |
| `sort` | `field`, `dir` (`asc`/`desc`) | مرتب‌سازی تک‌ستونه |
| `clearSorts` | — | حذف مرتب‌سازی‌ها |
| `page` | `page` | رفتن به صفحه؛ محدود به تعداد صفحات |
| `pageSize` | `pageSize` | اندازهٔ صفحه (۱ تا ۱۰۰۰) و بازگشت به صفحهٔ ۱ |
| `hideColumns` / `showColumns` | `fields[]` | مخفی/نمایان کردن ستون‌ها (حداقل یک ستون نمایان می‌ماند) |
| `showAllColumns` | — | نمایش همهٔ ستون‌ها |
| `select` | `field`, `value` | انتخاب ردیف منطبق در صفحهٔ جاری و رخداد `Change` |
| `highlight` | `field?`, `value` | هایلایت ردیف‌های منطبق؛ بدون `field` همهٔ ستون‌های نمایان بررسی می‌شوند |
| `clearHighlight` | — | پاک کردن هایلایت |
| `refresh` | — | بارگذاری مجدد |

کلاینت حداکثر ۲۰ دستور را اعمال می‌کند و هر دستور را در برابر متادیتای گرید
اعتبارسنجی می‌کند: فقط ستون‌های موجود، ستون‌های `sortable`/`filterable`،
عملگرهای مجاز و `pageSize` در بازهٔ ۱ تا ۱۰۰۰. مقدارها همیشه به‌صورت متن
مقایسه می‌شوند. این دستورها فقط UI را تغییر می‌دهند و هیچ عملیات داده‌ای
تخریبی در قرارداد وجود ندارد؛ با این حال endpoint را احراز هویت/مجوز دهید و
خروجی مدل را نامعتبر فرض کنید.

#### API سمت کلاینت

```js
const grid = Tech.Grid.get('usersGrid');

grid.chat.open();
grid.chat.close();
grid.chat.toggle();
grid.chat.isOpen();
grid.chat.send('صفحه بعد');          // Promise
grid.chat.messages();                 // [{ role, content }]

grid.setSort('FullName', 'desc');     // فقط state
grid.setSorts([{ field: 'City', dir: 'asc' }]);
grid.clearSorts();
grid.setPage(2);
grid.setPageSize(50);
grid.setColumnVisible('Email', false); // رندر فوری
grid.showAllColumns();
grid.highlight('City', 'تهران');       // رندر فوری
grid.clearHighlight();
grid.selectBy('Email', 'a@b.com');     // انتخاب از صفحهٔ جاری
grid.state();                          // { page, pageSize, sorts, filters, highlight }
```

مثل `setFilter`، متدهای `setSort`/`setSorts`/`clearSorts`/`setPage`/
`setPageSize` درخواستی نمی‌فرستند؛ بعد از آن‌ها `refresh()` را صدا بزنید.
`selectBy` در صورت پیدا نشدن مقدار در صفحهٔ جاری کاری انجام نمی‌دهد.

در هر دو نمونه، پارسر مشترک
[`src/samples/Shared/GridChatParser.cs`](../src/samples/Shared/GridChatParser.cs)
استفاده می‌شود: یک پارسر محلی فارسی بدون کلید و بدون شبکه که دستورهایی مثل
«فیلتر شهر تهران»، «مرتب‌سازی بر اساس نام صعودی»، «صفحه بعد»،
«ستون ایمیل را مخفی کن» و «شهر تهران را هایلایت کن» را به
`TechGridChatResponse` تبدیل می‌کند. صفحهٔ مشتریان در `TechUI.SampleAdmin` و
صفحهٔ اصلی `TechUI.SampleMvc` (گرید `usersGrid`) هر دو به آن متصل‌اند. برای
اتصال واقعی به مدل زبانی فقط همان endpoint را جایگزین کنید.

---

## DropDownList

```razor
@(Html.Tech().DropDownListFor<ProductViewModel, int?>(m => m.CategoryId)
    .OptionLabel("-- انتخاب کنید --")
    .Width(260)
    .DataTextField("Text")
    .DataValueField("Id")
    .Filterable()
    .FilterMode(FilterMode.StartsWith)
    .ServerFiltering()
    .MinLength(1)
    .DataSource(ds => ds.Read("GetCategories", "Home").Type(TechHttpMethod.Get))
    .Events(e => e
        .Change("onCategoryChange")
        .Open("onDropdownOpen")
        .Close("onDropdownClose")))
```

| متد | توضیح |
| --- | --- |
| `Name`, `Value`, `OptionLabel` | نام، مقدار اولیه، گزینه‌ی خالی |
| `Width(int)` | عرض dropdown |
| `DataTextField`, `DataValueField` | نام فیلد متن و مقدار در JSON |
| `Filterable`, `ServerFiltering`, `MinLength` | جستجو؛ `ServerFiltering` درخواست را با پارامتر `term` به سرور می‌فرستد |
| `FilterMode(FilterMode)` | `Contains` (پیش‌فرض) یا `StartsWith` |
| `CascadeFrom(string parentName)` | وابستگی به مقدار dropdown والد |
| `CascadeParentField(string field)` | نام پارامتر ارسالی به سرور (پیش‌فرض `parentId`) |
| `Events(...)` | `Change`, `Open`, `Close`, `DataBound` |

`FilterMode` در namespace `TechUI.Mvc.DropDownList` است.

### API سمت کلاینت

```js
const dropdown = Tech.DropDownList.get('CategoryId');

dropdown.value();
dropdown.text();
dropdown.dataItem();          // آیتم کامل JSON
dropdown.setValue('2');
await dropdown.reload();      // بارگذاری مجدد از DataSource

// میان‌برهای سراسری
Tech.DropDownList.value('CategoryId');
Tech.DropDownList.text('CategoryId');
Tech.DropDownList.dataItem('CategoryId');
Tech.DropDownList.setValue('CategoryId', '2');
```

رویداد `Change` مقدار `{ sender, value }` را می‌دهد.

---

## ورودی‌های متنی

### TextBox

```razor
@(Html.Tech().TextBoxFor<MyModel, string?>(m => m.Email)
    .Type("email")
    .Placeholder("name@example.com")
    .PrefixIcon("envelope")
    .Clearable()
    .InputMode("email")
    .AutoComplete("off")
    .MaxLength(120)
    .ShowCounter())
```

اختصاصی: `Type(string)`, `Clearable(bool = true)`, `InputMode(string)`,
`AutoComplete(string)`.

### TextArea

```razor
@(Html.Tech().TextAreaFor<MyModel, string?>(m => m.Bio)
    .Rows(4)
    .AutoGrow()
    .MaxLength(400)
    .ShowCounter())
```

اختصاصی: `Rows(int)`, `AutoGrow(bool = true)`.

### NumericTextBox

```razor
@(Html.Tech().NumericTextBoxFor<MyModel, decimal>(m => m.Price)
    .Culture("fa-IR")
    .Decimals(0)
    .Grouping(true)
    .Min(0)
    .Max(1_000_000_000)
    .Step(1000)
    .SpinButtons())
```

اختصاصی: `Min(decimal)`, `Max(decimal)`, `Step(decimal)`, `Decimals(int)`,
`Grouping(bool)`, `Culture(string)`, `SpinButtons(bool = true)`.

### MaskedTextBox

```razor
@(Html.Tech().MaskedTextBoxFor<MyModel, string?>(m => m.Mobile)
    .Mask("0000-000-0000")
    .UnmaskedValue())
```

`0` رقم، `9` رقم اختیاری و `A`/`a` حرف را نشان می‌دهند. `UnmaskedValue(true)`
مقدار بدون جداکننده‌ها را به سرور می‌فرستد (پیش‌فرض فعال است).

### Editor

```razor
@(Html.Tech().EditorFor<MyModel, string?>(m => m.Content)
    .Placeholder("متن خود را وارد کنید...")
    .Height(240)
    .MaxLength(2000)
    .ShowCounter()
    .Tools("bold", "italic", "ul", "link"))
```

`Tools` از میان `bold`, `italic`, `underline`, `ul`, `ol`, `link`, `image`,
`file`, `clear` انتخاب می‌کند. خروجی HTML در یک input مخفی با نام مدل ارسال
می‌شود.

```js
const editor = Tech.Editor.get('Content');
editor.value();
editor.setValue('<p>متن جدید</p>');
```

### آپلود و انتخاب تصویر/فایل در Editor

```razor
<form asp-action="Editor" method="post">
    @(Html.Tech().EditorFor<MyModel, string?>(m => m.Content)
        .Upload(Url.Action("Upload", "Home"))          // هر دو نوع
        .MediaLibrary(Url.Action("MediaLibrary", "Home"))
        .MaxUploadSize(10 * 1024 * 1024)
        .UploadField("file"))
</form>
```

| متد | کاربرد |
| --- | --- |
| `ImageUpload(url)` | آدرس POST آپلود تصویر |
| `FileUpload(url)` | آدرس POST آپلود فایل |
| `Upload(url)` | یک آدرس برای هر دو |
| `MediaLibrary(url)` | آدرس GET فهرست رسانه‌ها |
| `UploadField(name)` | نام فیلد فایل در FormData (پیش‌فرض `file`) |
| `MaxUploadSize(bytes)` | سقف حجم فایل در سمت کلاینت |

دکمه‌های «تصویر» و «فایل» فقط وقتی نمایش داده می‌شوند که آدرس آپلود یا
کتابخانه تنظیم شده باشد.

**قرارداد سرور:**

- `POST` به شکل `multipart/form-data` با فیلد فایل و فیلد `kind`
  (`image` یا `file`) ارسال می‌شود و توکن anti-forgery به‌صورت فیلد
  `__RequestVerificationToken` همراه درخواست است (فرم باید
  `asp-action`/`@Html.AntiForgeryToken()` داشته باشد).
- پاسخ موفق باید JSON با شکل `{ "url": "...", "name": "...", "size": 123, "type": "image" }`
  باشد. `url` باید نسبی (`/...`) یا `http(s)` باشد.
- `MediaLibrary` با پارامتر `?type=image|file` فراخوانی می‌شود و آرایه‌ای از
  همان ساختار را برمی‌گرداند.
- `MaxUploadSize` فقط بررسی سمت کلاینت است؛ محدودیت واقعی را باید سرور
  اعمال کند.

تصویر به‌صورت `<img class="tech-editor-image">` و فایل به‌صورت لینک
`<a class="tech-editor-file" download>` در محل نشانگر درج می‌شود.

---

## CheckBox و RadioButton

```razor
@(Html.Tech().CheckBoxFor<MyModel, bool>(m => m.IsActive)
    .Label("فعال"))

@(Html.Tech().CheckBoxFor<MyModel, bool>(m => m.NewsLetter)
    .Label("عضویت در خبرنامه")
    .Switch())

@(Html.Tech().CheckBox("Locked")
    .Label("قفل‌شده")
    .ReadOnly()
    .Value(true))

@(Html.Tech().RadioButtonFor<MyModel, string?>(m => m.Gender)
    .Items(items => items
        .Add("1", "مرد")
        .Add("2", "زن")
        .Add("3", "سایر"))
    .Inline())
```

اختصاصی CheckBox: `Label(string)`, `Switch(bool = true)`.

در CheckBox متصل به یک `bool` غیرنال‌پذیر، حالت unchecked معتبر است و به‌صورت
`false` ارسال می‌شود؛ بنابراین اعتبارسنجی «الزامی» برای آن اعمال نمی‌شود.
اختصاصی RadioButton: `Items(Action<ChoiceItemsBuilder>)` یا
`Items(IEnumerable<KeyValuePair<string, string>>)`, `Inline(bool = true)`.

`ChoiceItemsBuilder`: `Add(value, text)`, `Add(text)` (مقدار = متن)،
`AddRange(...)`.

رویداد `Change` مقدار `{ sender, checked, value }` را می‌دهد.

---

## DatePicker

تقویم جلالی، میلادی یا ترکیبی. در حالت ترکیبی کاربر با دکمهٔ کوچک داخل پنل بین
دو تقویم جابه‌جا می‌شود. مقدار مخفی فرم، خروجی `Tech.DatePicker.value` و payload
رویداد همیشه ISO میلادی (`yyyy-MM-dd`) است و به‌صورت مستقیم به `DateTime` سمت
سرور bind می‌شود. فقط متن نمایشی فیلد با `Format` و تقویم فعال ساخته می‌شود.

```razor
@using TechUI.Mvc.Inputs

@(Html.Tech().DatePickerFor<MyModel, DateTime?>(m => m.BirthDate)
    .Format("yyyy/MM/dd")
    .Min(new DateTime(1900, 1, 1))
    .Max(DateTime.Today)
    .TodayButton()
    .ClearButton()
    .Calendar(DatePickerCalendar.Both)
    .OpenOnFocus())
```

اختصاصی: `Format(string)`, `Min(DateTime)`, `Max(DateTime)`,
`TodayButton(bool = true)`, `ClearButton(bool = true)`,
`Calendar(DatePickerCalendar)`, `OpenOnFocus(bool = true)`.

`DatePickerCalendar` سه حالت دارد:

| مقدار | رفتار |
| --- | --- |
| `Jalali` | فقط تقویم شمسی؛ دکمهٔ تغییر تقویم ندارد |
| `Gregorian` | فقط تقویم میلادی؛ دکمهٔ تغییر تقویم ندارد |
| `Both` (پیش‌فرض) | هر دو تقویم با دکمهٔ شمسی/میلادی |

در حالت ثابت، `setCalendar` و دکمهٔ پنل کاری نمی‌کنند و تقویم روی همان حالت
قفل می‌ماند. هر دو تقویم هفته را از شنبه شروع می‌کنند و `Min`/`Max` روی مقدار
میلادی اعمال می‌شوند.

```js
const picker = Tech.DatePicker.get('BirthDate');

picker.value();                       // ISO میلادی، مثلاً '2026-03-21'
picker.setValue('1990-05-20');        // ISO میلادی
picker.calendar();                    // 'jalali' | 'gregorian'
picker.setCalendar('gregorian');      // در حالت ثابت نادیده گرفته می‌شود
picker.open();
picker.close();

Tech.DatePicker.value('BirthDate');              // همان ISO
Tech.DatePicker.setCalendar('BirthDate', 'jalali');
```

همین مقدار از DOM و jQuery هم در دسترس است؛ شناسهٔ `BirthDate` روی input
نمایشی است و مقدار میلادی در hidden input کنار آن قرار دارد:

```js
document.querySelector('#BirthDate')
    .closest('.tech-datepicker')
    .querySelector('input[type="hidden"]').value;   // ISO میلادی

$('#BirthDate').closest('.tech-datepicker')
    .find('input[type="hidden"]').val();            // ISO میلادی

$('#BirthDate').val();                              // متن نمایشی
```

رویداد `Change`: `{ sender, value, display, calendar }`؛ `value` همان ISO
میلادی، `display` متن نمایشی و `calendar` تقویم فعال (`jalali` یا
`gregorian`) است. سمت سرور نیز مستقیماً `<input type="hidden" name="BirthDate">`
را با همان ISO دریافت می‌کند.

---

## DateRangePicker

دو فیلد شروع/پایان با یک پنل مشترک (دو ماه نمایش هم‌زمان). مثل DatePicker
می‌تواند جلالی، میلادی یا ترکیبی باشد. در حالت ترکیبی کاربر با دکمهٔ کوچک داخل
پنل بین دو تقویم جابه‌جا می‌شود. مقدار دو hidden input، خروجی
`Tech.DateRangePicker.value` و payload رویداد همیشه ISO میلادی (`yyyy-MM-dd`)
است و مستقیماً به `DateTime` سمت سرور bind می‌شود. فقط متن نمایشی فیلدها با
`Format` و تقویم فعال ساخته می‌شود.

```razor
@using TechUI.Mvc.Inputs

@(Html.Tech().DateRangeFor<ReportModel, DateTime?, DateTime?>(
        m => m.FromDate, m => m.ToDate)
    .Placeholders("تاریخ شروع", "تاریخ پایان")
    .Min(new DateTime(2020, 1, 1))
    .Max(DateTime.Today)
    .TodayButton()
    .ClearButton()
    .Calendar(DatePickerCalendar.Both))

@* بدون مدل *@
@(Html.Tech().DateRange("ReportFrom", "ReportTo")
    .Values("2026-10-01", "2026-10-31")
    .Calendar(DatePickerCalendar.Gregorian)
    .Format("MM/dd/yyyy")
    .TodayButton(false))
```

| متد | توضیح |
| --- | --- |
| `Start(object?)` / `End(object?)` / `Values(start, end)` | مقدار اولیه |
| `Placeholders(start, end)` | متن راهنمای دو فیلد |
| `Format(string)` | قالب نمایشی متن فیلدها |
| `Calendar(DatePickerCalendar)` | تقویم شمسی، میلادی یا ترکیبی |
| `Min(DateTime)` / `Max(DateTime)` | محدوده‌ی مجاز |
| `TodayButton` / `ClearButton` / `OpenOnFocus` | کنترل دکمه‌ها و رفتار باز شدن |

`DatePickerCalendar` سه حالت دارد:

| مقدار | رفتار |
| --- | --- |
| `Jalali` | فقط تقویم شمسی؛ دکمهٔ تغییر تقویم ندارد |
| `Gregorian` | فقط تقویم میلادی؛ دکمهٔ تغییر تقویم ندارد |
| `Both` (پیش‌فرض) | هر دو تقویم با دکمهٔ شمسی/میلادی |

در حالت ثابت، `setCalendar` و دکمهٔ پنل کاری نمی‌کنند و تقویم روی همان حالت
قفل می‌ماند. هر دو تقویم هفته را از شنبه شروع می‌کنند و `Min`/`Max` روی مقدار
میلادی اعمال می‌شوند.

هر دو فیلد مخفی با نام‌های مشخص‌شده به سرور ارسال می‌شوند (ISO میلادی).
اعتبارسنجی روی فیلد شروع اعمال می‌شود.

```js
const range = Tech.DateRangePicker.get('ReportFrom');

range.value();                                  // { start, end } به ISO میلادی
range.setValue('2026-10-01', '2026-10-31');     // ISO میلادی
range.calendar();                               // 'jalali' | 'gregorian'
range.setCalendar('gregorian');                 // در حالت ثابت نادیده گرفته می‌شود
range.open();
range.close();

Tech.DateRangePicker.value('ReportFrom');
Tech.DateRangePicker.setCalendar('ReportFrom', 'jalali');
```

همین مقدار از DOM و jQuery هم در دسترس است؛ شناسهٔ فیلد شروع روی input نمایشی
است و مقدار میلادی در دو hidden input کنار آن قرار دارد:

```js
const root = document.querySelector('#ReportFrom')
    .closest('.tech-daterange');

root.querySelectorAll('input[type="hidden"]');   // [start, end] به ISO میلادی

$('#ReportFrom').closest('.tech-daterange')
    .find('input[type="hidden"]').map(function () { return this.value; }).get();

$('.tech-daterange-start').val();                // متن نمایشی شروع
```

رویداد `Change`: `{ sender, start, end, calendar }`؛ مقادیر `start`/`end`
همان ISO میلادی و `calendar` تقویم فعال (`jalali` یا `gregorian`) است. اگر
کاربر ابتدا پایان را انتخاب کند، خودکار به شروع تبدیل می‌شود و تاریخ‌های معکوس
جابه‌جا می‌شوند.

---

## TimePicker

```razor
@(Html.Tech().TimePickerFor<MyModel, TimeSpan?>(m => m.StartTime)
    .Step(15)
    .Use12Hour()
    .Min(new TimeSpan(8, 0, 0))
    .Max(new TimeSpan(18, 0, 0))
    .NowButton()
    .ClearButton())
```

اختصاصی: `Step(int minutes)`, `ShowSeconds(bool = true)`,
`Use12Hour(bool = true)`, `Min(TimeSpan)`, `Max(TimeSpan)`,
`NowButton(bool = true)`, `ClearButton(bool = true)`.

مقدار مخفی با قالب `HH:mm` (یا `HH:mm:ss` در حالت ثانیه) ارسال می‌شود.

```js
const time = Tech.TimePicker.get('StartTime');
time.value();
time.setValue('08:30');
time.open();
time.close();
```

---

## MultiSelect و Tag Input

```razor
@(Html.Tech().MultiSelectFor<MyModel, List<string>>(m => m.Roles)
    .Placeholder("نقش‌ها را انتخاب کنید")
    .Filterable()
    .Items(items => items
        .Add("1", "مدیر سیستم")
        .Add("2", "اپراتور"))
    .Events(e => e.Change("onRolesChange")))

@* جستجوی سمت سرور *@
@(Html.Tech().MultiSelectFor<MyModel, List<int>>(m => m.CategoryIds)
    .Placeholder("دسته‌ها را جستجو کنید")
    .Filterable()
    .ServerFiltering()
    .MinLength(1)
    .DataTextField("Text")
    .DataValueField("Id")
    .DataSource(ds => ds.Read("GetDropDownList", "Home")))

@* Tag Input با مقادیر آزاد *@
@(Html.Tech().MultiSelectFor<MyModel, List<string>>(m => m.Tags)
    .Placeholder("برچسب بنویسید و Enter بزنید")
    .Filterable(false)
    .AllowCustom())
```

| متد | توضیح |
| --- | --- |
| `Value` / `Values(params)` / `Values(IEnumerable)` | مقادیر اولیه |
| `Items(Action<ChoiceItemsBuilder>)` / `Items(IEnumerable<KeyValuePair<string, string>>)` | گزینه‌ها |
| `Placeholder`, `Filterable`, `ServerFiltering`, `MinLength` | جستجو |
| `AllowCustom(bool = true)` | افزودن مقدار دلخواه با Enter |
| `MaxItems(int)` | سقف تعداد انتخاب |
| `Disabled(bool = true)` | غیرفعال |
| `DataTextField`, `DataValueField`, `DataSource(...)` | منبع داده |
| `Events(...)` | `Change`, `Open`, `Close`, `DataBound` |

مقادیر انتخاب‌شده به‌صورت input‌های مخفی متعدد با همان نام مدل ارسال
می‌شوند؛ برای `List<string>` و `List<int>` آماده است.

```js
const ms = document.querySelector('.tech-multiselect')._techMultiSelect;

ms.getValues();                          // ['1', '2']
ms.getItems();                           // [{ value, text }]
ms.setValues(['1', '3']);
ms.add('4', 'مدیر مالی');
ms.remove('1');
ms.clear();
ms.open();
ms.close();
ms.reload();                             // بارگذاری مجدد از DataSource
```

رویداد `Change`: `{ sender, values, items }`.

---

## Tabs

```razor
@(Html.Tech().Tabs("settingsTabs")
    .Item("اعلان‌ها", "<p>...</p>", active: true)
    .Item("امنیت", new HtmlString("<p>...</p>"))
    .Active(0)
    .Pills()
    .Change("onTabChanged"))
```

| متد | توضیح |
| --- | --- |
| `Item(title, content[, active])` | `content` می‌تواند `string` یا `IHtmlContent` باشد |
| `Active(int)` | تعیین تب فعال |
| `Pills(bool = true)` | ظاهر pill |
| `Change(string)` | نام تابع رویداد |
| `CssClass`, `Attribute` | سفارشی‌سازی |

اگر `id` نداده باشید یک شناسه‌ی یکتا (`tech-tabs-xxxxxxxx`) تولید می‌شود و در
`TabId` قابل خواندن است.

```js
const tabs = Tech.Tabs.get('settingsTabs');
tabs.select(1);
tabs.active();                  // ایندکس تب فعال
```

رویداد `Change`: `{ sender, index, tab, panel }`. پیمایش با کلید شامل
`ArrowRight`/`ArrowLeft` (RTL)، `Home` و `End` است.

---

## Accordion

```razor
@(Html.Tech().Accordion("faqAccordion")
    .Item("سؤال اول", "<p>پاسخ...</p>", expanded: true)
    .Item("سؤال دوم", new HtmlString("<p>پاسخ...</p>"))
    .Multiple()
    .Flush()
    .Change("onAccordionChanged"))
```

| متد | توضیح |
| --- | --- |
| `Item(title, content[, expanded])` | یک آیتم |
| `Multiple(bool = true)` | باز بودن هم‌زمان چند آیتم |
| `Flush(bool = true)` | حذف فاصله/حاشیه‌ها |
| `Change(string)` | نام تابع رویداد |
| `CssClass`, `Attribute` | سفارشی‌سازی |

```js
const accordion = Tech.Accordion.get('faqAccordion');
accordion.toggle(1);
accordion.open(1);
accordion.close(0);
```

رویداد `Change`: `{ sender, index, open }`.

---

## TreeView

```razor
@(Html.Tech().TreeView("categories")
    .Items(items => items
        .Add("digital", "کالای دیجیتال", node => node
            .Icon("phone")
            .Expanded()
            .Data("count", 3)
            .Children(children => children
                .Add("mobile", "موبایل", child => child
                    .Data("count", 2))
                .Add("laptop", "لپ‌تاپ", child => child
                    .Disabled())))
        .Add("books", "کتاب"))
    .CheckBoxes()
    .Selection("multiple")           // single | multiple | none
    .ExpandAll()
    .ClientTemplate("<b>#: text #</b> <span>#: count #</span>")
    .Select("onTreeSelect")
    .Check("onTreeCheck"))
```

| متد | توضیح |
| --- | --- |
| `Items(Action<TreeViewNodeFactory>)` | ساخت درخت؛ `Add(id, text, configure)` |
| `CheckBoxes(bool = true)` | چک‌باکس با آبشار به فرزندان و حالت نامعین والد |
| `Selection("single"\|"multiple"\|"none")` | حالت انتخاب نود (پیش‌فرض `single`) |
| `ExpandAll(bool = true)` | باز بودن همه نودها هنگام بارگذاری |
| `ClientTemplate(string)` | تمپلیت HTML سمت کلاینت (سینتکس Kendo) |
| `ClientHtml(string)` | نام جایگزین `ClientTemplate` |
| `Select(string)` \| `Check(string)` | نام تابع رویداد انتخاب/تیک |
| `CssClass`, `Attribute` | سفارشی‌سازی |

متدهای نود: `Icon`, `Value`, `Expanded`, `Disabled`, `Selected`, `Checked`,
`Data(key, value)` و `Children(...)`.

### تمپلیت کلاینت

برچسب هر نود در مرورگر با تمپلیت بازنویسی می‌شود:

- `#: field #` خروجی را **HTML-encode** می‌کند.
- `#= field #` خروجی **خام** می‌گذارد (فقط برای دادهٔ مورد اعتماد).
- `field` می‌تواند `id`, `text`, `value`, `icon`, `expanded`, `disabled`,
  `checked`, `selected` یا هر کلید ثبت‌شده با `Data` باشد. پیشوند `data.`
  هم پذیرفته می‌شود (`#: data.count #`).

### API کلاینت

```js
const tree = Tech.TreeView.get('categories');

tree.expandAll();
tree.collapseAll();
tree.expand('digital');
tree.collapse('digital');
tree.select('mobile');
tree.selectedIds();
tree.checkedIds();
tree.getNode('mobile');   // { id, text, value, icon, ...data }
```

| رویداد | Payload |
| --- | --- |
| `Select` | `{ sender, id, text, selectedIds }` |
| `Check` | `{ sender, id, checked }` |

رویدادها تابع سراسری نام‌داده‌شده را صدا می‌زنند (مانند بقیه کامپوننت‌ها).

پیمایش کیبورد: `ArrowUp`/`ArrowDown`, `Home`/`End`, `Enter`/`Space` برای
انتخاب، و `ArrowRight`/`ArrowLeft` (با در نظر گرفتن RTL) برای باز/بستن.
ساختار ARIA (`role="tree"`, `treeitem`, `aria-expanded`, `aria-selected`)
به‌صورت سرور و کلاینت مدیریت می‌شود.

---

## DropDownTree

انتخاب مقدار از یک ساختار درختی داخل dropdown؛ مناسب دسته‌بندی، سازمان،
محل و هر دادهٔ سلسله‌مراتبی.

```razor
@(Html.Tech().DropDownTreeFor<MyModel, string?>(m => m.Category)
    .Id("categoryTree")
    .Placeholder("انتخاب دسته‌بندی...")
    .Filterable()
    .Clearable()
    .ClientTemplate("<b>#: text #</b> <span>#: count #</span>")
    .Items(items => items
        .Add("digital", "کالای دیجیتال", node => node
            .Icon("phone")
            .Expanded()
            .Data("count", 3)
            .Children(children => children
                .Add("mobile", "موبایل")
                .Add("laptop", "لپ‌تاپ")))
        .Add("books", "کتاب"))
    .Select("onCategorySelect")
    .Change("onCategoryChange"))

@* چندتایی با چک‌باکس آبشاری *@
@(Html.Tech().DropDownTreeFor<MyModel, List<string>>(m => m.AreaIds)
    .CheckBoxes()
    .ExpandAll()
    .Filterable()
    .Items(items => items
        .Add("shipping", "ارسال", node => node.Children(children => children
            .Add("express", "پست پیشتاز")
            .Add("normal", "پست عادی")))
        .Add("support", "پشتیبانی"))
    .Change("onAreasChange"))
```

| متد | توضیح |
| --- | --- |
| `DropDownTree(name)` \| `DropDownTreeFor(expr)` | ساخت کنترل؛ اگر نوع مدل `IEnumerable` باشد خودکار چندتایی می‌شود |
| `Items(Action<TreeViewNodeFactory>)` | نودهای درخت؛ همان API `TreeView` |
| `Value(string)` \| `Values(...)` | مقدار(های) انتخاب‌شده |
| `Selection("single"\|"multiple")` | حالت انتخاب (پیش‌فرض `single`) |
| `CheckBoxes(bool = true)` | فقط در حالت چندتایی؛ آبشار به فرزندان و حالت نامعین والد |
| `ExpandAll(bool = true)` | باز بودن همه نودها هنگام بارگذاری |
| `Placeholder(string)` | متن حالت خالی |
| `Clearable(bool = true)` | دکمهٔ پاک‌کردن مقدار |
| `Filterable(bool = true)` | جستجوی کلاینتی داخل درخت (بدون درخواست سرور) |
| `Disabled(bool)` \| `ReadOnly(bool)` | غیرفعال / فقط‌خواندنی |
| `Width(int)` | عرض کنترل به پیکسل |
| `CloseOnSelect(bool)` | بستن پنل بعد از انتخاب (پیش‌فرض: تکی `true`، چندتایی `false`) |
| `ClientTemplate(string)` \| `ClientHtml(string)` | تمپلیت Kendo برای برچسب نودها |
| `ClientTemplate` سینتکس | `#: field #` encode شده و `#= field #` خام |
| `Select`, `Change`, `Check`, `Open`, `Close` | نام تابع رویداد |
| `Id`, `CssClass`, `Attribute` | سفارشی‌سازی |

متدهای نود همان `TreeViewNode` هستند: `Icon`, `Value`, `Expanded`, `Disabled`,
`Selected`, `Checked`, `Data(key, value)` و `Children(...)`.

### فرم و اعتبارسنجی

- حالت تکی یک `input[type=hidden]` با `name` کنترل و حالت چندتایی برای هر مقدار
  یک hidden با همان `name` رندر می‌کند؛ بنابراین مدل‌های `string` و
  `List<string>`/`string[]` مستقیماً bind می‌شوند.
- `Required` و سایر Data Annotations روی همان hidden اعمال می‌شوند و پیام
  `tech-validation-message` بعد از کنترل قرار می‌گیرد. اعتبارسنجی کلاینت با
  `Tech.validate(form)` هنگام submit و پس از هر تغییر مقدار اجرا می‌شود.
- پنل از موقعیت‌دهی شناور استفاده می‌کند، پس داخل `modal` و نواحی اسکرول‌دار
  بریده نمی‌شود.

### API کلاینت

```js
const tree = Tech.DropDownTree.get('categoryTree');

tree.open();
tree.close();
tree.value();          // تکی: string | چندتایی: string[]
tree.values();
tree.texts();
tree.selectedIds();
tree.select('mobile'); // شناسه یا مقدار نود
tree.clear();
tree.getNode('mobile'); // { id, text, value, icon, ...data }
```

| رویداد | Payload |
| --- | --- |
| `Select` | `{ sender, id, text, value, values, selectedIds }` |
| `Change` | `{ sender, values, texts, value }` (در چندتایی `value` برابر `null`) |
| `Check` | `{ sender, id, checked }` (`checked` آرایهٔ شناسه‌هاست) |
| `Open` \| `Close` | `{ sender }` |

پیمایش کیبورد: روی کنترل `Enter`/`Space`/`ArrowDown` باز می‌کند، `Escape`
می‌بندد و `Backspace`/`Delete` مقدار تکی را پاک می‌کند. داخل پنل
`ArrowUp`/`ArrowDown`, `Home`/`End` حرکت، `Enter`/`Space` انتخاب،
`ArrowRight`/`ArrowLeft` (RTL-aware) باز/بستن و `Escape` بستن پنل است.
با `Filterable`، فوکوس روی جستجو قرار می‌گیرد و `ArrowDown` به اولین نتیجه
می‌رود.

---

## FileUpload

ناحیهٔ انتخاب و رهاکردن فایل با اعتبارسنجی پسوند، حجم و تعداد، پیش‌نمایش
تصویر، نوار پیشرفت و دو حالت ارسال: آپلود AJAX یا فرم بومی.

```razor
@(Html.Tech().FileUpload("attachments")
    .Multiple()
    .Accept(".png", ".jpg", ".pdf", ".zip")
    .MaxSizeMb(10)
    .MaxFiles(5)
    .Url(Url.Action("Upload", "Home"))
    .Events(events => events
        .Change("onFilesChanged")
        .Success("onFileUploaded")
        .Error("onUploadFailed")
        .Complete("onUploadComplete")))
```

اگر `Url` داده شود، هر فایل جداگانه با `XMLHttpRequest` به همان آدرس
`POST` می‌شود (فرم `multipart/form-data`، فیلد پیش‌فرض `file`) و توکن
آنتی‌فورجری به‌صورت هدر `RequestVerificationToken` و فیلد
`__RequestVerificationToken` ارسال می‌شود. با موفقیت، مقدار بازگشتی سرور
در یک `input type="hidden"` با نام بیلدر (`attachments`) قرار می‌گیرد.

اگر `Url` داده نشود، فایل‌ها فقط سمت کلاینت انتخاب و از طریق همان
`input type="file"` با ارسال معمولی فرم به سرور می‌رسند؛ نام فیلد همان
نام بیلدر است. برای این حالت `enctype="multipart/form-data"` را روی فرم
فراموش نکنید.

### گزینه‌ها

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Name(string)` | نام سازنده | نام فیلد فرم |
| `Id(string)` | همان نام | شناسهٔ ریشه و مبنای شناسهٔ zone و input |
| `Multiple(bool)` | `false` | انتخاب چند فایل؛ در حالت تک‌فایل، فایل جدید جای قبلی را می‌گیرد |
| `Accept(string)` / `Accept(params string[])` | — | پسوند (`.pdf`) یا MIME (`image/*`، `image/png`)؛ هم روی `accept` بومی و هم اعتبارسنجی کلاینت اعمال می‌شود |
| `MaxSize(long)` / `MaxSizeMb(int)` | بدون محدودیت | حداکثر حجم هر فایل |
| `MaxFiles(int)` | بدون محدودیت | حداکثر تعداد فایل هم‌زمان |
| `Url(string)` | — | آدرس اکشن آپلود؛ بدون آن حالت فرم بومی فعال می‌شود |
| `Field(string)` | `file` | نام فیلد فایل در درخواست AJAX |
| `AutoUpload(bool)` | `true` | ارسال خودکار بلافاصله پس از انتخاب؛ با `false` دکمهٔ «شروع آپلود» نمایش داده می‌شود |
| `Thumbnails(bool)` | `true` | پیش‌نمایش تصویر برای فایل‌های تصویری |
| `ShowFileList(bool)` | `true` | نمایش فهرست فایل‌ها |
| `DragDrop(bool)` | `true` | فعال‌بودن رهاکردن فایل روی ناحیه |
| `Disabled(bool)` | `false` | غیرفعال‌کردن انتخاب و آپلود |
| `Required(bool)` | `false` | الزام حداقل یک فایل موفق هنگام ارسال فرم |
| `Title(string)` | متن پیش‌فرض فارسی | عنوان ناحیه |
| `Hint(string)` | خودکار از Accept/حجم/تعداد | متن راهنما |
| `BrowseText(string)` | «انتخاب فایل» | متن دکمهٔ انتخاب |
| `Events(Action<FileUploadEventBuilder>)` | — | ثبت رویدادها |
| `Attribute` / `Attributes` | — | ویژگی‌های اضافی روی ریشه |

### رویدادها

| رویداد | payload |
| --- | --- |
| `Change` | `{ files }` — فهرست فایل‌های انتخاب‌شده |
| `Upload` | `{ file }` — شروع آپلود یک فایل |
| `Success` | `{ file, response }` — پاسخ JSON سرور |
| `Error` | `{ file, message, response }` |
| `Complete` | `{ files }` — پایان صف آپلود |
| `Remove` | `{ file }` — حذف فایل از فهرست |

هر آیتم `file` شامل `id`، `name`، `size`، `type`، `status`
(`pending`، `uploading`، `done`، `ready`، `error`)، `progress`، `value`
و `url` است.

### قرارداد پاسخ سرور

پاسخ موفق می‌تواند یک رشته یا شیء JSON باشد. اگر شیء باشد، اولین مقدار
غیرخالی از `id`، `value` یا `name` در فیلد مخفی قرار می‌گیرد و `url`
(اختیاری) در API کلاینت قابل خواندن است. برای خطا، پاسخ غیر 2xx با یکی
از فیلدهای `message`، `error` یا `title` به‌صورت پیام زیر همان فایل
نمایش داده می‌شود.

### API کلاینت

```js
const upload = Tech.FileUpload.get('attachments');

upload.getFiles();          // فهرست فایل‌ها و وضعیت‌ها
upload.addFiles(fileList);  // افزودن FileList یا آرایهٔ File
upload.upload();            // ارسال فایل‌های در انتظار
upload.remove(id);          // حذف یک فایل
upload.clear();             // پاک‌کردن کل فهرست
```

### نکته‌ها

- در حالت AJAX نام از input خام حذف می‌شود تا فرم، فایل را دوباره ارسال
  نکند؛ مقادیر موفق از فیلدهای مخفی خوانده می‌شوند.
- در حالت بومی، انتخاب‌ها با `DataTransfer` داخل input بومی نگه داشته
  می‌شوند تا ارسال فرم کامل باشد. مرورگرهای بسیار قدیمی که
  `DataTransfer` را پشتیبانی نمی‌کنند فقط از انتخاب‌گر بومی استفاده
  می‌کنند.
- `Required` هنگام submit در کلاینت بررسی می‌شود (با پیام فارسی)؛ بدون
  جاوااسکریپت، در حالت بومی ویژگی `required` روی input باقی می‌ماند. اگر
  آپلود AJAX هنوز در جریان باشد، ارسال فرم تا پایان آپلود متوقف می‌شود.
- اعتبارسنجی پسوند، حجم و تعداد در کلاینت فقط برای تجربهٔ کاربری است؛
  سرور باید همین قواعد را مستقل بررسی کند.
- استایل‌ها از توکن‌های `--tech-*` استفاده می‌کنند و با تم تاریک،
  قالب‌های رنگی و `.tech-rtl` سازگارند.

## Captcha

تصویر امنیتی سمت سرور برای فرم‌ها. چالش روی سرور ساخته می‌شود، پاسخ داخل
یک توکن رمزنگاری‌شده (AES-GCM) قرار می‌گیرد و تصویر به‌صورت SVG برمی‌گردد؛
بنابراین بدون Session یا state سمت سرور کار می‌کند و پاسخ در HTML یا URL
قابل خواندن نیست.

### راه‌اندازی سرور

در `Program.cs`:

```csharp
using TechUI.Mvc.Captcha;

builder.Services.AddTechUICaptcha(options =>
{
    options.Secret = builder.Configuration["TechUI:Captcha:Secret"] ?? "";
    options.ExpiresIn = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

app.MapTechUICaptcha();   // GET /techui/captcha
```

| گزینه | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Secret` | خالی | کلید مشتق‌سازی AES. خالی یعنی کلید تصادفی برای هر پروسه؛ در این حالت توکن‌های صادرشده بعد از ری‌استارت برنامه بی‌اعتبار می‌شوند. در محیط عملیاتی مقدار پایدار (user-secrets یا متغیر محیطی) بدهید. |
| `ExpiresIn` | ۵ دقیقه | عمر هر چالش؛ مقدار صفر یا منفی به ۵ دقیقه برمی‌گردد. |
| `ImageWidth` / `ImageHeight` | `190` / `58` | ابعاد SVG؛ به ترتیب در بازهٔ ۱۴۰..۴۰۰ و ۴۴..۱۲۰ محدود می‌شوند. |
| `Noise` | `Medium` | سطح پیش‌فرض نویز تصویر؛ هر کپچا می‌تواند با `Noise(...)` آن را بازنویسی کند. |

`MapTechUICaptcha(pattern)` یک endpoint از نوع `GET` ثبت می‌کند که پاسخ JSON
`{ token, image, expiresAt }` با هدر `Cache-Control: no-store` برمی‌گرداند و
پارامترهای `language`, `kind`, `operation`, `length` و `noise` را می‌پذیرد.
دکمهٔ بازخوانی تصویر از همین آدرس استفاده می‌کند.

### استفاده در فرم

```razor
@using TechUI.Mvc.Captcha

<form method="post">
    @Html.AntiForgeryToken()

    @(Html.Tech().Captcha("Captcha")
        .Language(CaptchaLanguage.Persian)
        .Kind(CaptchaKind.Letters)
        .Noise(CaptchaNoiseLevel.Medium)
        .Length(5)
        .Required()
        .ValidationError(ModelState["Captcha"]?.Errors
            .FirstOrDefault()?.ErrorMessage))

    <button type="submit" class="btn btn-primary">ارسال</button>
</form>
```

خروجی شامل تصویر، دکمهٔ «تصویر جدید»، input متنی با `name` بیلدر و یک
input مخفی `{name}Token` است که توکن امضاشده را نگه می‌دارد. تصویر `alt` و
متن‌های پیش‌فرض بر اساس زبان انتخاب‌شده (فارسی/انگلیسی) تعیین می‌شوند.

### گزینه‌های بیلدر

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Name(string)` | نام سازنده | نام فیلد پاسخ؛ فیلد توکن `{name}Token` می‌شود |
| `Language(CaptchaLanguage)` | `Persian` | الفبا و ارقام نمایش‌داده‌شده |
| `Kind(CaptchaKind)` | `Letters` | نوع چالش: حروف، ارقام یا محاسبه |
| `MathOperation(CaptchaMathOperation)` | `Addition` | عملگر چالش ریاضی |
| `Noise(CaptchaNoiseLevel)` | پیش‌فرض سرور | میزان نویز تصویر: `None`, `Low`, `Medium`, `High` |
| `Length(int)` | `5` | تعداد حروف/ارقام؛ به بازهٔ ۳ تا ۸ محدود می‌شود و برای `Math` نادیده گرفته می‌شود |
| `Placeholder(string)` | متن پیش‌فرض زبان | متن راهنمای input |
| `RefreshUrl(string)` | `/techui/captcha` | آدرس endpoint بازخوانی |
| `CssClass(string)` / `Attribute(name, value)` | — | کلاس و ویژگی اضافه روی ریشه |
| `Disabled(bool)` | `false` | غیرفعال‌کردن input و دکمهٔ بازخوانی |
| `Required(bool)` | `false` | افزودن `required` به input |
| `ValidationError(string?)` | — | نمایش پیام خطا زیر کنترل همراه با `is-invalid` و `aria-invalid` |

### انواع چالش

| enum | مقادیر | رفتار |
| --- | --- | --- |
| `CaptchaLanguage` | `Persian`, `English` | حروف فارسی/انگلیسی؛ ارقام در حالت فارسی به‌صورت ۰..۹ فارسی و در حالت انگلیسی لاتین نمایش داده می‌شوند |
| `CaptchaKind` | `Letters`, `Digits`, `Math` | حروف تصادفی، ارقام تصادفی یا سؤال ریاضی |
| `CaptchaMathOperation` | `Addition`, `Subtraction`, `Multiplication` | جمع (۲..۹ + ۲..۹)، تفریق (۶..۱۲ منهای عددی کوچک‌تر از آن) و ضرب (۲..۹ × ۲..۹) |
| `CaptchaNoiseLevel` | `None`, `Low`, `Medium`, `High` | تعداد خطوط، منحنی‌ها و نقطه‌های نویز و دامنهٔ چرخش متن؛ `Medium` پیش‌فرض است و `None` تصویر کاملاً تمیز می‌دهد |

حروف جداگانه و نه به‌صورت یک متن یکنواخت رسم می‌شوند: هر کاراکتر جای عمودی،
زاویه و اندازهٔ کمی متفاوت دارد تا چیدمان قابل پیش‌بینی نباشد. با بالا رفتن
سطح نویز، خط‌ها، منحنی‌ها و نقطه‌های بیشتری روی تصویر کشیده می‌شود و `High`
بیشترین چگالی و جابه‌جایی را دارد.

در الفبای انگلیسی حروف مبهم `I` و `O` حذف شده‌اند.

### اعتبارسنجی سمت سرور

```csharp
[HttpPost]
public async Task<IActionResult> Captcha(CaptchaSampleModel model)
{
    var result = await Request.ValidateTechCaptchaAsync(nameof(model.Captcha));

    if (!result.IsValid)
    {
        ModelState.AddModelError(nameof(model.Captcha), result.Error!);
        return View(model);
    }

    // ...
}
```

`ValidateTechCaptcha(fieldName)` نسخهٔ همگام است (فرم را از `Request.Form`
می‌خواند) و `ValidateTechCaptchaAsync` فرم را async می‌خواند. نام فیلد پیش‌فرض
`"Captcha"` است. پیام‌های خطا فارسی و تفکیک‌شده‌اند: توکن نامعتبر، انقضای
زمان، پاسخ خالی و پاسخ نادرست.

پاسخ ارسالی قبل از مقایسه نرمال می‌شود: ارقام فارسی و عربی به لاتین تبدیل
می‌شوند، فاصله و نویسه‌های ZWNJ/ZWJ حذف می‌شوند، `ي`/`ك` عربی به `ی`/`ک`
نگاشت می‌شود و مقایسه به بزرگی/کوچکی حروف حساس نیست.

### API کلاینت

```js
const captcha = Tech.Captcha.get('Captcha');

captcha.answer();         // مقدار واردشده
captcha.token();          // توکن مخفی جاری
await captcha.refresh();  // دریافت تصویر و توکن جدید

Tech.Captcha.refresh('Captcha');  // میان‌بر
```

بازخوانی، input را پاک و خطای قبلی را حذف می‌کند و زبان، نوع چالش و سطح
نویز را از `data-*` همان کنترل حفظ می‌کند. خطای شبکه یا سرور با
رویداد `Error` روی ریشه و payload `{ error }` اعلام می‌شود؛ نام تابع گیرنده
را با `data-event-error` تنظیم کنید:

```razor
@(Html.Tech().Captcha("Captcha")
    .Attribute("data-event-error", "onCaptchaError"))
```

### نکته‌ها

- توکن stateless است؛ نیازی به Session نیست، اما در چند سرور باید `Secret`
  مشترک تنظیم شود.
- پاسخ داخل توکن رمزنگاری‌شده است و از DOM یا URL قابل استخراج نیست.
- `Noise(None)` برای خوانایی بیشتر یا دسترس‌پذیری است و به‌تنهایی جلوی ربات
  را نمی‌گیرد؛ برای فرم‌های عمومی حداقل `Low` یا `Medium` را نگه دارید.
- کپچا جایگزین rate limiting نیست؛ endpoint ارسال فرم را همچنان محدود کنید.
- استایل‌ها از توکن‌های `--tech-*` استفاده می‌کنند و با تم تاریک، پریست‌ها،
  RTL و `.tech-rtl` سازگارند.

## Wizard / Stepper

ناوبری مرحله‌به‌مرحله برای فرم‌های چندبخشی. هر مرحله یک پنل مستقل دارد،
نوار پیشرفت موقعیت مرحلهٔ جاری را نشان می‌دهد (با «قبلی» کاهش می‌یابد و
پس از Finish به ۱۰۰٪ می‌رسد) و در حالت خطی فقط مراحل طی‌شده قابل بازگشت‌اند.
مراحل می‌توانند محتوای متنی (HTML) یا هر `IHtmlContent` دیگر، مانند خروجی
یک Partial View، باشند.

```razor
@(Html.Tech().Wizard("checkoutWizard")
    .Step("اطلاعات مشتری", customerStep, "نام و ایمیل", "person")
    .Step("اطلاعات شرکت", companyStep, "شرکت و نوع حساب", "building")
    .Step("مرور و تأیید", "<p>بررسی نهایی</p>", null, "check2-circle")
    .PrevText("قبلی")
    .NextText("بعدی")
    .FinishText("ثبت نهایی")
    .SubmitOnFinish()
    .Events(e => e
        .Change("onWizardChange")
        .Complete("onWizardComplete")
        .Finish("onWizardFinish")))
```

`Step` دو overload دارد: `Step(string title, string content, ...)` که رشته را
به‌صورت HTML خام رندر می‌کند و `Step(string title, IHtmlContent content, ...)`
برای محتوای ساخته‌شده. `icon` نام آیکن Bootstrap Icons بدون پیشوند `bi-` است.
شمارهٔ مرحله به‌صورت خودکار محاسبه می‌شود.

### گزینه‌ها

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Wizard(string? id)` | شناسهٔ خودکار | شناسهٔ ریشه و مبنای شناسهٔ دکمه‌ها و پنل‌ها |
| `Step(...)` | — | افزودن مرحله (عنوان، محتوا، توضیح، آیکن) |
| `Active(int)` | `0` | مرحلهٔ فعال اولیه (صفرمبنا)؛ مراحل قبل تکمیل‌شده در نظر گرفته می‌شوند |
| `Linear(bool)` | `true` | در حالت خطی پرش به مراحل بعدی و دکمه‌های آن‌ها غیرفعال است |
| `Vertical(bool)` | `false` | چیدمان عمودی فهرست مراحل |
| `Progress(bool)` | `true` | نمایش نوار پیشرفت |
| `Controls(bool)` | `true` | نمایش دکمه‌های قبلی/بعدی/پایان |
| `SubmitOnFinish(bool)` | `false` | ارسال نزدیک‌ترین فرم وقتی مرحلهٔ آخر با «پایان» تأیید شود |
| `PrevText` / `NextText` / `FinishText` | «قبلی» / «بعدی» / «پایان» | متن دکمه‌ها |
| `Events(Action<WizardEventBuilder>)` | — | ثبت رویدادها |
| `CssClass` / `Attribute` / `Attributes` | — | استایل و ویژگی‌های اضافی روی ریشه |

### اعتبارسنجی مرحله

قبل از رفتن به مرحلهٔ بعد (و پیش از Finish) فقط همان پنل با
`Tech.validate(panel)` بررسی می‌شود؛ بنابراین ورودی‌های ساخته‌شده با
`Html.Tech()` یا `asp-for` که ویژگی‌های `data-val="true"` دارند به‌صورت
خودکار پوشش داده می‌شوند. در صورت خطا، کلاس `has-error` روی مرحله اضافه
می‌شود و رویداد `tech:wizard-invalid` با `detail: { index }` روی ریشه منتشر
می‌شود. با اولین ورودی/تغییر در همان پنل، حالت خطا پاک می‌شود.

استفاده از `SubmitOnFinish` اعتبارسنجی سمت کلاینت را جایگزین اعتبارسنجی
سرور نمی‌کند؛ در اکشن، `ModelState` را مستقل بررسی کنید.

### رویدادها

| رویداد | payload |
| --- | --- |
| `Change` | `{ index, from, step, panel, state }` — تغییر مرحلهٔ فعال |
| `Complete` | `state` — تکمیل مرحلهٔ آخر |
| `Finish` | `state` — پایان ویزارد (پس از `Complete`) |

`state` برابر است با:

```js
{
    index: 1,             // مرحلهٔ جاری (صفرمبنا)
    total: 3,
    last: false,
    completed: [true, false, false],
    steps: [{ index, title, active, completed }]
}
```

### API کلاینت

```js
const wizard = Tech.Wizard.get('checkoutWizard');

wizard.next();          // اعتبارسنجی و رفتن به بعدی (در آخرین مرحله: finish)
wizard.prev();          // مرحلهٔ قبل
wizard.goTo(2);         // رفتن به مرحلهٔ دلخواه؛ در حالت خطی پرش رو به جلو مجاز نیست
wizard.finish();        // تکمیل و اجرای رویدادها (و ارسال فرم در صورت SubmitOnFinish)
wizard.reset();         // بازگشت به مرحلهٔ اولیه و پاک‌کردن تکمیل‌ها
wizard.current();       // شمارهٔ مرحلهٔ جاری
wizard.state();         // وضعیت کامل

Tech.Wizard.goTo('checkoutWizard', 0); // میان‌بر
```

`Tech.Wizard.get` هم شناسه و هم خود عنصر ریشه را می‌پذیرد. شناسهٔ مرحله
`{id}-step-{i}`، شناسهٔ پنل `{id}-panel-{i}` و مرحلهٔ جاری با
`aria-current="step"` مشخص می‌شود. دکمه‌های مراحل در حالت خطی برای مراحل
بعدی `disabled` هستند و پنل فعال پس از هر تغییر فوکوس می‌گیرد.

### نکته‌ها

- `Active` برای شروع از مرحلهٔ میانی است؛ مراحل قبل از آن تکمیل‌شده
  نمایش داده می‌شوند.
- بازگشت به مرحلهٔ قبل، وضعیت تکمیل مراحل را پاک نمی‌کند؛ فقط نوار پیشرفت
  بر اساس مرحلهٔ جاری کاهش می‌یابد.
- در حالت آزاد (`Linear(false)`) پرش به هر مرحله، از جمله از طریق دکمه‌های
  هدر، مجاز است؛ ولی هنگام ارسال فرم، `Tech.wireValidation` همهٔ فیلدها را
  بررسی می‌کند.
- چیدمان عمودی برای ستون‌های باریک مناسب است. در عرض‌های کمتر از
  `576px` فهرست افقی به‌صورت قابل اسکرول نمایش داده می‌شود.
- استایل‌ها از توکن‌های `--tech-*` استفاده می‌کنند و با تم تاریک،
  قالب‌های رنگی و RTL سازگارند.

## Timeline

فهرست عمودی رویدادها برای تاریخچهٔ سفارش، فعالیت تیم یا لاگ تغییرات. هر
آیتم مارکر آیکن‌دار (یا نقطه)، عنوان، زمان، توضیح و برچسب اختیاری دارد و
رنگ مارکر با `tone` کنترل می‌شود.

```razor
@using TechUI.Mvc.Timeline

@(Html.Tech().Timeline("orderTimeline")
    .Item("تحویل داده شد", "امروز — ۱۱:۰۲",
        "سفارش در محل مشتری تحویل شد.",
        "check2-circle", TimelineTones.Success, "پایان")
    .Item("ارسال شد", "دیروز — ۰۹:۱۵",
        "کد رهگیری ۲۴۱۹۸۷",
        "truck", TimelineTones.Info)
    .Item("سفارش ثبت شد", "۱۴۰۵/۰۸/۱۴ — ۱۶:۰۵",
        "سفارش توسط سارا احمدی ثبت شد.",
        "receipt", TimelineTones.Neutral))
```

برای بدنهٔ HTML سفارشی (مثلاً لینک) از overload محتوایی استفاده کنید؛ متن
داخل `IHtmlContent` خام رندر می‌شود، پس ورودی کاربر را encode کنید:

```razor
@(Html.Tech().Timeline()
    .Item("گزارش آماده شد", "۱ ساعت پیش",
        new HtmlString("گزارش مهرماه آماده است؛ <a href=\"#reports\">مشاهده</a>."),
        "file-earmark-bar-chart", TimelineTones.Info))
```

### گزینه‌ها

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Timeline(string? id)` | — | شناسهٔ ریشه (اختیاری) |
| `Item(title, time, description, icon, tone, badge)` | — | افزودن رویداد؛ همهٔ پارامترها بعد از `title` اختیاری‌اند |
| `Item(title, time, IHtmlContent content, icon, tone, badge)` | — | رویداد با بدنهٔ HTML سفارشی |
| `Compact(bool)` | `false` | فاصله، اندازهٔ مارکر و فونت فشرده‌تر |
| `CssClass` / `Attribute` / `Attributes` | — | استایل و ویژگی‌های اضافی روی ریشه |

آیکن بدون پیشوند `bi-` نوشته می‌شود (پیشوند `bi-` هم پذیرفته می‌شود).
`tone` یکی از مقادیر `TimelineTones.Primary`، `Success`، `Info`، `Warning`،
`Danger` یا `Neutral` است؛ مقدار ناشناخته به ظاهر خنثی برمی‌گردد.

### نکته‌ها

- خروجی از `ol/li` استفاده می‌کند و مارکرها `aria-hidden` هستند تا برای
  صفحه‌خوان تکرار نشوند.
- `time` با `dir="auto"` رندر می‌شود تا اعداد و متن فارسی/لاتین درست بچینند.
- کامپوننت جاوااسکریپت ندارد؛ کاملاً سرور-رندر است. فقط `techui.css` لازم است.
- `Primary` از توکن `--tech-primary` استفاده می‌کند و با قالب‌های رنگی هماهنگ
  می‌شود؛ رنگ‌های دیگر در تم تاریک نسخهٔ روشن‌تر دارند.

## Breadcrumb

مسیر راهنمای صفحه به‌صورت کامل سمت سرور. خروجی یک `nav` با `aria-label` و
فهرست `ol/li` است؛ آخرین آیتم بدون لینک و با `aria-current="page"` رندر
می‌شود.

```razor
@(Html.Tech().Breadcrumb("shopBreadcrumb")
    .Home("/")
    .Item("فروشگاه", "/shop", "shop")
    .Item("لوازم جانبی", "/shop/accessories", "headphones")
    .Item("هدفون بی‌سیم پرو", null, "earbuds"))
```

### گزینه‌ها

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Breadcrumb(string? id)` | — | شناسهٔ ریشه (اختیاری) |
| `Item(text, href, icon)` | — | افزودن آیتم؛ `href` و `icon` اختیاری‌اند |
| `Home(href, text, icon)` | `/`، «خانه»، `house` | میان‌بر آیتم اول |
| `Separator(string?)` | — | جداکنندهٔ سفارشی به‌جای فلش پیش‌فرض |
| `Compact(bool)` | `false` | فاصله و فونت فشرده‌تر |
| `Label(string)` | «مسیر صفحه» | مقدار `aria-label` روی `nav` |
| `CssClass` / `Attribute` / `Attributes` | — | استایل و ویژگی‌های اضافی روی ریشه |

آیکن بدون پیشوند `bi-` نوشته می‌شود (پیشوند `bi-` هم پذیرفته می‌شود). آیتم
بدون `href` به‌صورت متن رندر می‌شود و فقط آیتم آخر به‌عنوان صفحهٔ جاری علامت
می‌خورد.

### نکته‌ها

- جداکنندهٔ پیش‌فرض در LTR به سمت راست و در RTL به سمت چپ است (آینه‌شدن
  خودکار کاراکتر یونیکد)؛ `Separator` آن را در هر دو جهت جایگزین می‌کند.
- متن طولانی صفحهٔ جاری با نقطه‌چین کوتاه می‌شود و به خط بعد نمی‌شکند.
- کامپوننت جاوااسکریپت ندارد؛ فقط `techui.css` لازم است.
- رنگ‌ها از توکن‌های تم می‌آیند، پس در تم تاریک و قالب‌های رنگی بدون تنظیم
  اضافی درست نمایش داده می‌شود.

## Chat

پنل گفتگوی مستقل با دو حالت:

- **`TechChatMode.Assistant`** — هر پیام به `Url` ارسال می‌شود و پاسخ در همان
  درخواست برمی‌گردد (`Reply`). مناسب اتصال به LLM یا ربات FAQ.
- **`TechChatMode.Support`** — پیام به صندوق کارشناس تحویل داده می‌شود و
  پاسخ‌ها با `PollUrl` دریافت می‌شوند؛ یا با
  `Tech.Chat.get(id).receive(...)` از SignalR/WebSocket به‌صورت realtime
  تزریق می‌شوند.

```razor
@using TechUI.Mvc.Chat

@Html.AntiForgeryToken()

@(Html.Tech().Chat("supportChat")
    .Mode(TechChatMode.Support)
    .Url(Url.Action("ChatSupport", "Home")!)
    .PollUrl(Url.Action("ChatSupportPoll", "Home")!)
    .PollInterval(3)
    .Title("پشتیبانی فروشگاه")
    .Subtitle("کارشناس آنلاین")
    .Welcome("سلام! پیام خود را بنویسید.")
    .Placeholder("متن پیام…")
    .Height(520)
    .Suggestions("پیگیری سفارش", "مرجوعی و تعویض"))
```

### گزینه‌ها

| متد | پیش‌فرض | توضیح |
| --- | --- | --- |
| `Chat(string? id)` | — | شناسهٔ ریشه (اختیاری) |
| `Mode(TechChatMode)` | `Assistant` | حالت گفتگو |
| `Url(string)` | — | endpoint دریافت پیام (الزامی) |
| `PollUrl(string)` | — | endpoint GET برای دریافت پاسخ کارشناس |
| `PollInterval(int)` | `4` | فاصلهٔ polling به ثانیه (۲ تا ۱۲۰) |
| `Title` / `Subtitle` | وابسته به حالت | عنوان و زیرعنوان هدر |
| `Welcome` / `Placeholder` | وابسته به حالت | متن خوش‌آمد و placeholder |
| `Icon(string)` | `stars` یا `headset` | آیکن اواتار بدون پیشوند `bi-` |
| `MaxLength(int)` | `1000` | حداکثر طول پیام (۱ تا ۴۰۰۰) |
| `Height(int)` | `520` | ارتفاع پنل به پیکسل (۲۸۰ تا ۱۲۰۰) |
| `Suggestions(params string[])` | — | پیشنهادهای شروع گفتگو |
| `ClearButton(bool)` | `true` | نمایش دکمهٔ پاک‌کردن گفتگو |
| `OnSend` / `OnReceive` / `OnClear` / `OnError` | — | نام تابع سراسری رویداد |
| `CssClass` / `Attribute` / `Attributes` | — | استایل و ویژگی‌های اضافی روی ریشه |

### قرارداد سرور

پیام با `POST` به‌صورت JSON ارسال می‌شود (هدر `RequestVerificationToken`
همراه است؛ پس `@Html.AntiForgeryToken()` را در صفحه بگذارید یا توکن را با
`.Attribute("data-antiforgery", token)` بدهید):

```json
{
  "message": "وضعیت سفارش ۱۰۲۴",
  "conversationId": "8f5c…",
  "sender": null,
  "history": [ { "role": "user", "content": "سلام" } ]
}
```

پاسخ حالت Assistant:

```json
{ "reply": "کد رهگیری را بفرستید…", "sender": "دستیار" }
```

در حالت Support، `POST` می‌تواند فقط تحویل را تأیید کند و پاسخ‌ها از
`PollUrl?conversation=…&after=<lastId>` برگردند:

```json
{
  "typing": true,
  "messages": [
    { "id": 12, "role": "agent", "sender": "مریم", "content": "سلام!" }
  ]
}
```

مدل‌های آماده: `TechChatRequest`، `TechChatMessage` و `TechChatResponse` در
`TechUI.Mvc.Chat`. نمونهٔ اتصال به مدل واقعی:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ChatAssistant([FromBody] TechChatRequest request)
{
    var reply = await _ai.AskAsync(request.Message, request.History);

    return Json(new TechChatResponse { Reply = reply, Sender = "دستیار" });
}
```

### اتصال به DeepSeek (نمونهٔ کامل)

نمونهٔ `TechUI.SampleMvc` اکشن `ChatAssistant` را به
[DeepSeek](https://platform.deepseek.com) وصل می‌کند. حساب جدید اعتبار رایگان
اولیه دارد و API آن با قرارداد OpenAI سازگار است
(`POST https://api.deepseek.com/chat/completions`). پیاده‌سازی کامل در
[`DeepSeekAssistant.cs`](../src/samples/TechUI.SampleMvc/Services/DeepSeekAssistant.cs)
است.

کلید را هرگز در سورس یا مخزن نگذارید. یکی از این دو روش:

```bash
dotnet user-secrets set "DeepSeek:ApiKey" "sk-..." --project src/samples/TechUI.SampleMvc

# یا متغیر محیطی
export DEEPSEEK_API_KEY="sk-..."
```

| کلید تنظیمات | پیش‌فرض | توضیح |
| --- | --- | --- |
| `DeepSeek:ApiKey` | — | کلید API؛ در نبود آن `DEEPSEEK_API_KEY` خوانده می‌شود |
| `DeepSeek:BaseUrl` | `https://api.deepseek.com` | آدرس پایه (بدون `/v1` هم کار می‌کند) |
| `DeepSeek:Model` | `deepseek-chat` | یا `deepseek-reasoner` |
| `DeepSeek:TimeoutSeconds` | `60` | مهلت درخواست (۵ تا ۱۲۰) |
| `DeepSeek:MaxTokens` | `1024` | سقف توکن پاسخ |
| `DeepSeek:SystemPrompt` | متن فارسی پیش‌فرض | دستور سیستمی دستیار |

هستهٔ فراخوانی ساده است: تاریخچهٔ گفتگو (فقط پیام‌های `user` و `assistant`،
حداکثر ۲۴ پیام) به‌همراه پیام جدید به `messages` اضافه می‌شود و پاسخ از
`choices[0].message.content` خوانده می‌شود:

```csharp
var payload = new Dictionary<string, object?>
{
    ["model"] = options.Model,
    ["messages"] = messages,   // system + history + user
    ["stream"] = false,
    ["temperature"] = 0.7,
    ["max_tokens"] = options.MaxTokens
};

using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
{
    Content = JsonContent.Create(payload)
};

httpRequest.Headers.Authorization = new("Bearer", options.ApiKey);

using var response = await http.SendAsync(httpRequest, cancellationToken);
var body = await response.Content.ReadAsStringAsync(cancellationToken);

var reply = JsonSerializer.Deserialize<CompletionResponse>(body, JsonOptions)
    ?.Choices?.FirstOrDefault()?.Message?.Content;
```

بدون کلید، پاسخ‌دهندهٔ محلی (`LocalAssistant`) جواب می‌دهد و همان صفحهٔ نمونه
با برچسب «حالت محلی» این وضعیت را نشان می‌دهد. اگر کلید تنظیم شده باشد ولی
سرویس خطا بدهد (مثلاً `401` یا اتمام اعتبار)، خطا در لاگ ثبت می‌شود و پاسخ
محلی با یادداشت کوتاهی از خطا به کاربر برمی‌گردد؛ بنابراین دمو هرگز قطع نمی‌شود.
استریم استفاده نمی‌شود و نشانگر تایپ تا رسیدن پاسخ کامل نمایش داده می‌شود.

### API کلاینت

```js
const chat = Tech.Chat.get('supportChat');

chat.send('سلام');
chat.receive({ role: 'agent', sender: 'مریم', content: 'در خدمتم' });
chat.typing(true);
chat.getMessages();
chat.clear();
chat.startPolling();
chat.stopPolling();
chat.focus();
chat.destroy();
```

رویدادها با `.OnSend("handler")` و مانند آن ثبت می‌شوند و payloadها عبارت‌اند
از `{ message }` برای `Send`، پیام نرمال‌شده برای `Receive`، `{}` برای `Clear`
و `{ error }` برای `Error`.

### نکته‌ها

- `Enter` پیام را می‌فرستد و `Shift+Enter` خط جدید می‌سازد؛ textarea تا سقف
  ۷rem رشد می‌کند.
- polling فقط وقتی صفحه visible است اجرا می‌شود و با بازگشت تب ادامه می‌یابد.
  برای حالت Support بدون `PollUrl`، پاسخ‌ها را با `receive` از SignalR بفرستید.
- محتوای پیام‌ها به‌صورت متن خام و escape‌شده نمایش داده می‌شود (خطوط حفظ
  می‌شوند)؛ Markdown رندر نمی‌شود.
- همهٔ رنگ‌ها از توکن‌های `--tech-*` می‌آیند؛ قالب‌های رنگی، تم تاریک و RTL
  بدون تنظیم اضافی روی حباب‌ها، هدر، اواتار و دکمهٔ ارسال اثر می‌گذارند.
- کامپوننت جاوااسکریپت دارد و از `data-tech-component="chat"` boot می‌شود؛
  برای HTML تزریقی بعد از لود، `Tech.boot()` را صدا بزنید. برای دسترسی با
  `Tech.Chat.get` حتماً یک `id` به `Chat(...)` بدهید.

## Chart

نمودار SVG بدون وابستگی؛ SVG کاملاً سمت کلاینت از `data-chart-options` ساخته
می‌شود و با تغییر اندازهٔ ظرف، تم و قالب رنگ دوباره رندر می‌شود.

```razor
@using TechUI.Mvc.Chart

@(Html.Tech().Chart("salesChart")
    .Type(ChartTypes.Column)
    .Title("فروش کانال‌ها")
    .Subtitle("سه ماه نخست")
    .Height(340)
    .Categories("فروردین", "اردیبهشت", "خرداد")
    .ValueFormat("number")
    .YAxis(axis => axis.Title("سفارش").Format("number"))
    .Series("حضوری", 320, 410, 380)
    .Series("آنلاین", 240, 280, 350)
    .Tooltip()
    .Legend(position: "bottom"))
```

اگر شناسه ندهید، شناسه‌ای با الگوی `tech-chart-xxxxxxxx` ساخته می‌شود. ارتفاع
پیش‌فرض بر اساس نوع (معمولاً ۳۴۰px، نمودار دایره‌ای ۳۲۰px، گیج ۲۶۰px و
sparkline ۶۴px) انتخاب می‌شود و `Height` بین ۱۲۰ و ۱۶۰۰ محدود می‌شود.

### انواع نمودار

نوع با `Type(ChartTypes.X)` یا متدهای میان‌بر `Horizontal()`، `Smooth()` و
`Area()` تنظیم می‌شود. ثابت‌های `TechUI.Mvc.Chart.ChartTypes`:

| ثابت | مقدار | کاربرد |
| --- | --- | --- |
| `Column` | `column` | ستونی (پیش‌فرض) |
| `Bar` | `bar` | میلهٔ افقی |
| `Line` | `line` | خطی |
| `Spline` | `spline` | خطی نرم |
| `Step` | `step` | خطی پله‌ای |
| `Area` | `area` | سطحی |
| `AreaSpline` | `areaspline` | سطحی نرم |
| `Pie` | `pie` | دایره‌ای |
| `Donut` | `donut` | حلقه‌ای |
| `Polar` | `polar` | قطبی |
| `Scatter` | `scatter` | نقطه‌ای (`Points`) |
| `Bubble` | `bubble` | حبابی (`Bubbles`) |
| `Radar` | `radar` | راداری |
| `Gauge` | `gauge` | گیج نیم‌دایره |
| `RadialBar` | `radialbar` | حلقهٔ پیشرفت |
| `Heatmap` | `heatmap` | نقشهٔ حرارتی |
| `Funnel` | `funnel` | قیفی |
| `Treemap` | `treemap` | درختی مساحتی |
| `Waterfall` | `waterfall` | آبشاری |
| `Candlestick` | `candlestick` | شمعی |
| `Bullet` | `bullet` | بولت |
| `Sparkline` | `sparkline` | جرقه‌ای خطی |
| `SparkBar` | `sparkbar` | جرقه‌ای میله‌ای |

نام‌های `area-spline`، `horizontalbar`، `hbar`، `radial`، `radial-gauge` و
`spark` هم به‌عنوان میان‌بر پذیرفته می‌شوند. برای ترکیب انواع، نوع را روی سری
بدهید:

```razor
@(Html.Tech().Chart("mixed")
    .Type(ChartTypes.Column)
    .Categories("فروردین", "اردیبهشت", "خرداد")
    .Series("فروش", 120, 180, 150)
    .Series(ChartTypes.Spline, "هدف", 150, 160, 170))
```

### سری‌ها

```razor
.Series(series => series
    .Title("سود")
    .Values(12, 18, null, 22)   // null = شکاف در خط
    .Color("#0ea5e9")
    .Dashed()
    .Opacity(0.25)
    .Stack("الف"))
```

| متد | توضیح |
| --- | --- |
| `Title(name)` | نام نمایشی در تولتیپ و لجند |
| `Type(type)` | نوع مستقل همین سری |
| `Values(params double?[])` | مقادیر دسته‌ها؛ `null` شکاف است |
| `Points(params ChartPoint[])` یا `Points((x, y)...)` | نقاط مختصات برای scatter/line |
| `Bubbles((x, y, r)...)` | حباب‌ها برای نمودار bubble |
| `Candles(params ChartCandle[])` | کندل‌ها (`Open`, `High`, `Low`, `Close`) |
| `Targets(params double?[])` | خط هدف هر دسته (bullet) |
| `Color(css)` | رنگ سفارشی این سری |
| `Dashed(bool)` | خط‌چین |
| `Opacity(double)` | شفافیت پرشدگی (۰..۱) |
| `Hidden(bool)` | شروع در حالت پنهان (در لجند قابل بازگردانی) |
| `Stack(name)` | نام گروه انباشت |

کلید `null` در `Values` برای نمودارهای خطی/ستونی شکاف ایجاد می‌کند. سری
پنهان از لجند حذف نمی‌شود؛ کاربر می‌تواند با کلیک دوباره آن را برگرداند.

### محورها

```razor
.XAxis(axis => axis
    .Title("ماه")
    .Rotate(-30)
    .Labels(false)
    .Grid(false))
.YAxis(axis => axis
    .Title("مبلغ")
    .Format("currency")
    .Min(0)
    .Max(100)
    .BeginAtZero())
```

`Show(bool)`, `Title(string)`, `Labels(bool)`, `Grid(bool)`,
`Format("number|percent|currency|compact")`, `Min`, `Max`, `BeginAtZero` و
`Rotate(-90..90)` در دسترس‌اند. برچسب‌ها با `Intl.NumberFormat` و لوکال صفحه
(`lang` نزدیک‌ترین والد یا `Locale`) قالب‌دهی می‌شوند؛ `percent` پسوند `٪` و
`currency` پسوند `ریال` می‌گیرد.

### گزینه‌های ظاهری و داده

| متد | توضیح |
| --- | --- |
| `Categories(params string[])` | برچسب دسته‌های محور |
| `Colors(params string[])` | پالت سفارشی (جایگزین متغیرهای CSS) |
| `Legend(bool show, string? position)` | نمایش لجند؛ `top` یا `bottom` |
| `Tooltip(bool show, string? format)` | تولتیپ؛ قالب مستقل از محور |
| `DataLabels(bool show)` | نمایش مقدار روی نقاط/میله‌ها |
| `Stacked(bool)` | انباشت سری‌ها |
| `Horizontal(bool)` | میلهٔ افقی |
| `Smooth(bool)` / `Area(bool)` | میان‌بر spline/area |
| `Rtl(bool)` | جهت محور عددی؛ پیش‌فرض از `direction` ظرف |
| `Height(int)` | ارتفاع به پیکسل |
| `ValueFormat(string)` | قالب پیش‌فرض مقادیر |
| `Locale(string)` | لوکال اعداد (مثلاً `fa-IR`) |
| `EmptyText(string)` | متن حالت بدون داده |
| `AriaLabel(string)` | برچسب دسترس‌پذیری |
| `Animation(bool)` | انیمیشن ورود (با `prefers-reduced-motion` غیرفعال می‌شود) |
| `CssClass(string)` / `Attribute(name, value)` | سفارشی‌سازی ریشه |

ویژهٔ بعضی انواع:

```razor
.Donut(hole: 0.64, centerText: "۱٬۸۴۰")   // donut
.Gauge(min: 0, max: 100, target: 85)      // gauge
.GaugeTarget(90)                           // تغییر هدف
.WaterfallTotal(false)                     // حذف ستون جمع
```

`donut` مقدار مرکزی و درصد هر برش، `gauge` باندهای رنگی و سوزن، `waterfall`
ستون «جمع» خودکار، و `bullet` خط هدف را رسم می‌کند.

### API کلاینت

```js
const chart = Tech.Chart.get('salesChart');

chart.render();                 // رندر دوباره
chart.update({ series: [ ... ] });
chart.destroy();

Tech.Chart.render('#anotherChart'); // فعال‌سازی دستی بعد از تزریق HTML
Tech.Chart.update('salesChart', { tooltip: { show: false } });
```

`update` گزینه‌های جدید را روی نمونهٔ فعلی ادغام (سطح اول) و دوباره رندر
می‌کند. هر نمونه `hiddenSeries` و `hiddenSlices` را برای وضعیت لجند نگه
می‌دارد. رندر با `ResizeObserver` (و رویداد `resize` پنجره به‌عنوان جایگزین) و
رویداد `tech:theme-changed` به‌روز می‌شود؛ نمودار در ظرف مخفی رندر نمی‌شود و
پس از نمایان‌شدن با اولین تغییر اندازه اصلاح می‌شود.

### رنگ‌ها و تم

پالت پیش‌فرض از متغیرهای CSS خوانده می‌شود:

```css
.tech-chart {
    --tech-chart-1: var(--tech-primary);
    --tech-chart-2: #059669;
    /* ... تا --tech-chart-8 */
    --tech-chart-positive: #16a34a;
    --tech-chart-negative: #dc2626;
    --tech-chart-warning: #f59e0b;
    --tech-chart-total: #64748b;
}
```

چون سری اول به `--tech-primary` وصل است، تغییر قالب رنگ (`Tech.theme.setPreset`)
رنگ نمودار را هم عوض می‌کند. تم تاریک مقادیر روشن‌تر پالت را جایگزین می‌کند.
اگر رنگ یک خانه با رنگ اصلی قالب یکی شود (مثلاً زمردی با `--tech-chart-2`)،
موتور نمودار آن را خودکار با یک رنگ متمایز جایگزین می‌کند تا سری‌ها قابل
تفکیک بمانند. برای رنگ‌های ثابت از `Colors` یا `Color` روی سری استفاده کنید.

---

## Map

نقشه‌ی تعاملی روی کاشی‌های OpenStreetMap (slippy map) بدون هیچ وابستگی
جاوااسکریپتی؛ موتور نقشه، نشانگرها، پاپ‌آپ‌ها، شکل‌های برداری، جستجو،
موقعیت‌یابی و کنترل‌ها همه در `techui.js` هستند. کاشی‌های تصویری از اینترنت
بارگذاری می‌شوند، بنابراین این کامپوننت بدون اتصال شبکه فقط پوسته‌ی خالی نشان
می‌دهد.

```razor
@using TechUI.Mvc.Map

@(Html.Tech().Map("tehranMap")
    .Height(480)
    .Center(35.71, 51.39)
    .Zoom(12)
    .MinZoom(4)
    .MaxZoom(19)
    .Marker(35.6997, 51.3380, marker => marker
        .Title("میدان آزادی")
        .Popup("<b>میدان آزادی</b><br />نماد شهر تهران")
        .Icon("building")
        .Tone(MapTones.Primary)
        .Open())
    .Marker(35.7522, 51.3756, "برج میلاد", "بلندترین برج ایران",
        "broadcast", MapTones.Info)
    .Polyline(new[] { (35.700, 51.410), (35.720, 51.400) },
        color: "#2563eb", weight: 5)
    .Polygon(new[] { (35.70, 51.38), (35.71, 51.38), (35.71, 51.39) },
        fillOpacity: 0.18)
    .Circle(35.7522, 51.3756, 1200, fillOpacity: 0.10)
    .Events(events => events
        .MarkerClick("onMapMarker")
        .MoveEnd("onMapMove")))
```

اگر شناسه ندهید، شناسه‌ای با الگوی `tech-map-xxxxxxxx` ساخته می‌شود. مرکز
پیش‌فرض تهران (`35.6892, 51.3890`)، زوم `12`، حداقل `2`، حداکثر `19` و ارتفاع
`420px` است. ارتفاع بین ۱۶۰ و ۲۰۰۰ پیکسل و زوم بین ۱ و ۲۲ محدود می‌شود.

### گزینه‌های بیلدر

| متد | توضیح |
| --- | --- |
| `Center(lat, lng)` | مرکز اولیه؛ عرض جغرافیایی به بازه‌ی وب‌مرکاتور و طول جغرافیایی به `-180..180` نرمال می‌شود |
| `Zoom(int)` / `MinZoom(int)` / `MaxZoom(int)` | زوم اولیه و محدوده‌ی زوم |
| `Height(int)` | ارتفاع ظرف به پیکسل |
| `TileUrl(url)` | آدرس کاشی پیش‌فرض؛ جای‌نگهدارهای `{z}`، `{x}`، `{y}`، `{s}` و `{-y}` پشتیبانی می‌شوند |
| `Attribution(html)` | متن انتساب گوشه‌ی نقشه |
| `Subdomains(params string[])` | زیردامنه‌ها برای جای‌نگهدار `{s}` |
| `Source(source, attribution?)` | انتخاب منبع کاشی از `MapSources` یا هر URL دارای `{z}` |
| `Layer(name, url, attribution?, selected?, maxZoom?)` | افزودن لایه‌ی پایه؛ اولین لایه به‌طور خودکار فعال می‌شود |
| `Layer(MapSource, name?, selected?)` | افزودن یکی از منابع آماده به‌عنوان لایه |
| `Marker(...)` | افزودن نشانگر (دو overload: ساده یا با `Action<MapMarkerBuilder>`) |
| `Polyline(points, color?, weight?, opacity?)` | خط شکسته |
| `Polygon(points, color?, weight?, opacity?, fillOpacity?)` | چندضلعی |
| `Circle(lat, lng, radiusMeters, color?, fillOpacity?)` | دایره با شعاع واقعی به متر |
| `Search(url?, placeholder?)` | فعال‌کردن جستجو (پیش‌فرض Nominatim) |
| `ZoomControl` / `ScaleControl` / `FullscreenControl` / `LocateControl` / `CoordinatesControl` / `LayerControl` / `SearchControl` | نمایش یا پنهان‌کردن هر کنترل (`bool show = true`) |
| `ScrollWheelZoom` / `DoubleClickZoom` / `Dragging` / `Keyboard` | فعال یا غیرفعال‌کردن تعامل (`bool enabled = true`) |
| `AriaLabel(string)` | برچسب دسترس‌پذیری ظرف نقشه |
| `CssClass(string)` / `Attribute(name, value)` / `Attributes(...)` | سفارشی‌سازی ریشه |

پیش‌فرض کنترل‌ها: زوم، مقیاس، تمام‌صفحه و موقعیت‌یابی روشن؛ مختصات خاموش؛
کنترل لایه فقط وقتی بیش از یک `Layer` ثبت شده باشد و کنترل جستجو فقط بعد از
فراخوانی `Search(...)` ظاهر می‌شود.

### منابع نقشه

کلاس `MapSources` چند منبع آماده دارد که URL، متن انتساب، زیردامنه‌ها و سقف
زوم را یک‌جا مشخص می‌کند:

| ثابت | کلید | منبع | حداکثر زوم |
| --- | --- | --- | --- |
| `MapSources.OpenStreetMap` | `osm` | کاشی استاندارد OSM | ۱۹ |
| `MapSources.CartoLight` | `light` | CARTO Voyager (روشن) | ۲۰ |
| `MapSources.CartoDark` | `dark` | CARTO Dark Matter | ۲۰ |
| `MapSources.OpenTopoMap` | `topo` | OpenTopoMap | ۱۷ |
| `MapSources.EsriSatellite` | `satellite` | تصاویر ماهواره‌ای Esri | ۱۹ |

```razor
// یک منبع برای کل نقشه
@(Html.Tech().Map("darkMap")
    .Source(MapSources.CartoDark)
    .Height(360))

// یا چند منبع به‌عنوان لایه‌ی قابل تعویض
@(Html.Tech().Map("layersMap")
    .Layer(MapSources.OpenStreetMap, selected: true)
    .Layer(MapSources.CartoLight)
    .Layer(MapSources.EsriSatellite))
```

`Source(string)` هم کلید/نام منبع را می‌پذیرد (`"satellite"`، `"توپوگرافی"`) و
هم هر URL کاشی دارای `{z}` را. `Source` سقف زوم منبع را اعمال می‌کند (برای
توپوگرافی ۱۷)؛ `MaxZoom(...)` بعد از آن می‌تواند سقف را تغییر دهد. در
`Layer(MapSource)` هم سقف همان لایه ذخیره می‌شود و هنگام فعال‌شدن لایه، زوم در
صورت نیاز تا سقف پایین کشیده می‌شود.

سیاست استفاده‌ی هر سرویس را پیش از ترافیک تولیدی بررسی کنید. برای سرور کاشی
اختصاصی از `Source(string)` یا `TileUrl(...)` استفاده کنید.

### نشانگرها

```razor
.Marker(35.6997, 51.3380, marker => marker
    .Id("azadi")
    .Title("میدان آزادی")
    .Popup("<b>میدان آزادی</b>")
    .Icon("geo-alt-fill")
    .Tone(MapTones.Danger)
    .Draggable()
    .Open())
```

| متد | توضیح |
| --- | --- |
| `Id(string)` | شناسه‌ی پایدار برای استفاده در API کلاینت |
| `Title(string)` | متن `title` و `aria-label` |
| `Popup(html)` | محتوای HTML پاپ‌آپ |
| `Icon(string)` | آیکون Bootstrap Icons بدون پیشوند `bi-` (پیش‌فرض `geo-alt-fill`) |
| `Tone(string)` | رنگ نشانگر از `MapTones`: `Primary`, `Success`, `Info`, `Warning`, `Danger`, `Neutral` |
| `Draggable(bool = true)` | جابه‌جایی نشانگر با درگ |
| `Open(bool = true)` | باز شدن پاپ‌آپ در بارگذاری اولیه |

محتوای `Popup` به‌صورت HTML درج می‌شود و باید از منبع مورد اعتماد بیاید؛ متن
نتایج جستجوی بیرونی قبل از نمایش escape می‌شود.

### رویدادها

متدهای `MapEventBuilder` نام یک تابع سراسری (`window[name]`) را می‌گیرند و تابع
با `(payload, api)` صدا زده می‌شود:

| متد | نام رویداد | payload |
| --- | --- | --- |
| `Click(handler)` | `click` | `{ lat, lng, point: { x, y } }` |
| `MoveEnd(handler)` | `moveEnd` | `{ center: { lat, lng }, zoom, bounds: { north, west, south, east } }` |
| `ZoomEnd(handler)` | `zoomEnd` | مثل `moveEnd` |
| `MarkerClick(handler)` | `markerClick` | `{ id, lat, lng, title }` |
| `MarkerDragEnd(handler)` | `markerDragEnd` | `{ id, lat, lng }` |
| `Locate(handler)` | `locate` | `{ lat, lng, accuracy }` یا `{ error: "unavailable" }` |
| `Search(handler)` | `search` | `{ query, results: [{ name, displayName, lat, lng }] }` |

رویدادهای `moveEnd`/`zoomEnd` پس از پایان حرکت یا زوم (با تأخیر کوتاه برای
جمع‌شدن حرکت پیوسته) و یک‌بار در پایان انیمیشن برنامه‌ای ارسال می‌شوند.

### API کلاینت

```js
const map = Tech.Map.get('tehranMap');

map.getCenter();                       // { lat, lng }
map.getZoom();
map.getBounds();                       // { north, west, south, east }
map.setView({ lat: 35.7, lng: 51.4 }, 14, true);
map.panTo({ lat: 35.7, lng: 51.4 }, true);
map.setZoom(13);
map.zoomIn();  map.zoomOut();
map.panBy(120, 0);                     // پیکسل
map.fitBounds([[25.05, 44.03], [39.78, 63.33]], { maxZoom: 6 });

const marker = map.addMarker({
    lat: 35.7, lng: 51.4, title: 'نقطه', popup: 'توضیح',
    icon: 'pin-angle', tone: 'success', draggable: true
});

marker.setLatLng(35.71, 51.41);
marker.getLatLng();
marker.setIcon('flag');
marker.openPopup();  marker.closePopup();
marker.remove();

map.removeMarker('marker-1');
map.getMarkers();
map.setBaseLayer('توپوگرافی');          // یا index یا URL
map.getBaseLayer();
map.locate();                          // Promise<{ lat, lng, accuracy } | null>
map.openPopup('marker-1');
map.closePopup();
map.invalidateSize();                  // بعد از تغییر اندازه یا نمایش ظرف مخفی
map.on('zoomEnd', handler);
map.off('zoomEnd', handler);
map.destroy();
```

`Tech.Map.get` هم شناسه و هم خود المان را می‌پذیرد و شیء API را برمی‌گرداند.
`fitBounds` آرایه‌ای از `[lat, lng]` یا `{ lat, lng }` می‌پذیرد و برای یک نقطه
زوم ۱۵ (یا `maxZoom`) را انتخاب می‌کند.

### نقشه، RTL و تم تاریک

موقعیت کنترل‌ها با ویژگی‌های منطقی CSS (`inset-inline-start/end`) تنظیم
می‌شود، پس در صفحه‌ی راست‌به‌چپ گروه زوم سمت راست و موقعیت‌یابی/تمام‌صفحه سمت
چپ قرار می‌گیرند؛ مقیاس و مقادیر مختصات با `direction: ltr` ایزوله می‌شوند تا
اعداد به‌هم نریزند. در تم تاریک کاشی‌ها با فیلتر تیره‌سازی نمایش داده می‌شوند و
کنترل‌ها و پاپ‌آپ از توکن‌های سطح استفاده می‌کنند، بنابراین قالب‌های رنگ هم روی
آن‌ها اثر می‌گذارند.

### نکته‌ها

- پیاده‌سازی مستقل است و به Leaflet/MapLibre وابسته نیست؛ فقط CSS و JS خود
  TechUI لازم است.
- کاشی پیش‌فرض `tile.openstreetmap.org` برای توسعه و ترافیک کم است. برای
  تولید، سرور کاشی یا سرویس تجاری خودتان را در `TileUrl`/`Layer` بگذارید و
  انتساب متناسب را در `Attribution` تنظیم کنید.
- جستجو با `format=jsonv2&limit=6&accept-language=fa` به `SearchUrl` ارسال
  می‌شود و نتایج در فهرست زیر کادر جستجو نمایش داده می‌شوند. برای Nominatim
  سیاست استفاده‌ی منصفانه (بدون درخواست انبوه) را رعایت کنید.
- ناوبری با صفحه‌کلید: `+`/`-` زوم، کلیدهای جهت‌دار جابه‌جایی و `Escape` بستن
  پاپ‌آپ/فهرست لایه/نتایج جستجو. با `Keyboard(false)` غیرفعال می‌شود.
- درگ با ماوس/لمس، پینچ دو انگشتی، زوم با چرخ ماوس (با لنگر روی موقعیت
  نشانگر ماوس) و دوبار کلیک پشتیبانی می‌شوند.
- در تم تاریک و حالت `prefers-reduced-motion` انیمیشن‌های نقشه سبک‌تر یا حذف
  می‌شوند.

---

## Spinner

```razor
@(Html.Tech().Spinner())
@(Html.Tech().Spinner().Size("sm").Color("light").Label("در حال ارسال..."))
```

`Size` از `xs`, `sm`, `md`, `lg`, `xl` و `Color` از
`primary`, `secondary`, `success`, `danger`, `warning`, `info`, `light`, `dark`
پشتیبانی می‌کند.

### Spinner روی یک ناحیه

```js
const overlay = Tech.Spinner.overlay('#userGrid', {
    size: 'md',
    color: 'primary',
    label: 'در حال بارگذاری...'
});

overlay.show();
overlay.hide();
overlay.remove();
```

`target` می‌تواند selector یا عنصر DOM باشد و خروجی شامل
`{ element, show, hide, remove }` است. کلاس `tech-spinner-host` برای
موقعیت‌دهی به عنصر هدف اضافه می‌شود.

---

## Toast / Notification

```js
const toast = Tech.notify('تغییرات ذخیره شد', {
    type: 'success',          // info | success | warning | danger
    title: 'ذخیره',
    duration: 4200,           // 0 = بدون بسته شدن خودکار
    position: 'top-left',     // top-left | top-right | bottom-left | bottom-right
    dismissible: true,
    progress: true
});

toast.close();
```

هنگام نگه‌داشتن موس روی toast تایمر متوقف و پس از خروج ادامه پیدا می‌کند.
`Tech.toast` نام دیگر `Tech.notify` است.

---

## Dialog / Confirm / Alert

### Dialog سفارشی

```js
const result = await Tech.dialog({
    title: 'حذف رکورد',
    message: 'آیا از حذف مطمئن هستید؟',
    type: 'danger',           // info | success | warning | danger | question
    size: 'sm',               // sm | lg
    closable: true,
    buttons: [
        { text: 'انصراف', value: false, variant: 'outline-secondary' },
        { text: 'حذف', value: true, variant: 'danger' },
        { text: 'ذخیره و بستن', action: close => { /* ... */ close(true); } }
    ]
});
```

برای محتوای HTML از `content` به‌جای `message` استفاده کنید. `onOpen` با
`{ dialog, close }` صدا زده می‌شود. بستن با Escape، کلیک روی backdrop یا دکمه‌ی
بستن مقدار `null` برمی‌گرداند.

### Confirm و Alert

```js
const ok = await Tech.confirm('رکورد حذف شود؟', {
    title: 'تأیید حذف',
    confirmText: 'حذف',
    cancelText: 'انصراف',
    confirmVariant: 'danger',
    type: 'warning'
});

await Tech.alert('عملیات انجام شد', {
    title: 'نتیجه',
    type: 'success',
    buttonText: 'باشه'
});
```

`Tech.confirm` همیشه boolean برمی‌گرداند و `Tech.alert` یک دیالوگ تک‌دکمه‌ای است.

---

## اعتبارسنجی فرم

1. با متدهای `*For` صفت‌های `data-val*` از DataAnnotations ساخته می‌شوند.
2. خطاهای `ModelState` همان لحظه روی کنترل و پیام اعتبارسنجی نمایش داده می‌شوند.
3. `Tech.boot()` فرم‌ها را `novalidate` کرده و در `submit` و
   `input`/`change` اعتبارسنجی زنده انجام می‌دهد.
4. پیام‌های پیش‌فرض انگلیسی به فارسی ترجمه می‌شوند (الزامی، طول، بازه، ایمیل،
   تلفن، URL، کارت بانکی، Regex و Compare).
5. کنترل‌های ترکیبی (تاریخ، ساعت، بازه، dropdown، multiselect، چک‌باکس/رادیو)
   از طریق input مخفی خود اعتبارسنجی می‌شوند تا با ModelState هم‌خوان بمانند.

API عمومی:

```js
Tech.validate(form);          // true/false و فوکوس روی اولین خطا
Tech.validateField(input);    // true/false برای یک فیلد
Tech.wireValidation(root);    // اتصال دستی فرم‌های تزریق‌شده
```

برای نمایش پیام کنار کنترل، کافی است `ValidationMessage()` را در زنجیره
بیاورید؛ span با `data-valmsg-for` به‌صورت خودکار ساخته می‌شود.

---

## RTL

کامپوننت‌ها از CSS logical properties استفاده می‌کنند. کلاس `tech-rtl` را روی
هر wrapper بگذارید (یا `dir="rtl"` را روی صفحه تنظیم کنید):

```html
<div class="tech-rtl">
    @(Html.Tech().DatePicker("BirthDate"))
</div>
```

---

## تم تاریک

```js
Tech.theme.set('dark');       // ذخیره در localStorage
Tech.theme.toggle();          // تغییر بین dark/light
Tech.theme.get();             // 'dark' | 'light'
Tech.theme.storageKey;        // 'tech-theme'
```

`Tech.theme.set` صفت `data-tech-theme` را روی `<html>` تنظیم می‌کند و رخداد
`tech:theme-changed` را با `detail.theme` و `detail.preset` ارسال می‌کند. برای
Bootstrap 5.3 هم صفت `data-bs-theme` را همگام کنید. اعمال استاتیک هم ممکن است:

```html
<html data-tech-theme="dark">
<div class="tech-dark">...</div>
```

---

## قالب‌های رنگ

پنج قالب رنگی علاوه بر قالب پیش‌فرض وجود دارد: `emerald`, `violet`, `rose`,
`amber`, `slate`. قالب فقط توکن‌های رنگ اصلی (accent) را عوض می‌کند و با حالت
تاریک و روشن هر دو ترکیب می‌شود.

```js
Tech.theme.presets;               // ['default','emerald','violet','rose','amber','slate']
Tech.theme.getPreset();           // 'default'
Tech.theme.setPreset('emerald');  // ذخیره در localStorage
Tech.theme.presetStorageKey;      // 'tech-theme-preset'
```

`setPreset` صفت `data-tech-preset` را روی `<html>` می‌گذارد. اعمال استاتیک:

```html
<html data-tech-preset="emerald">
<div class="tech-preset-violet">...</div>
```

قالب سفارشی با بازنویسی متغیرهای CSS:

```css
:root {
    --tech-primary: #0d9488;
    --tech-primary-hover: #0f766e;
    --tech-primary-rgb: 13,148,136;
    --tech-primary-soft: #ccfbf1;
    --tech-primary-soft-border: #99f6e4;
    --tech-primary-soft-text: #115e59;
    --tech-focus-border: #5eead4;
    --tech-focus-shadow: 0 0 0 .2rem rgba(13,148,136,.18);
    --tech-selected: var(--tech-primary-soft);
}
```

اگر TechUI را همراه Bootstrap استفاده می‌کنید، برای هم‌رنگ شدن دکمه‌ها و
نشان‌های Bootstrap با قالب، `--bs-primary-rgb` را به `--tech-primary-rgb` وصل
کنید (در نمونه‌ی `TechUI.SampleAdmin` همین کار انجام شده است).

---

## Template Helpers در Grid

داخل `Template` می‌توانید از `{{ }}` برای دستور یا خروجی ساده، `{{= }}` برای
خروجی و helper‌های زیر استفاده کنید. خروجی `{{ }}` و `{{= }}` به‌صورت HTML
خام درج می‌شود (برای رندر badge و لینک)؛ اگر داده‌ی کاربر می‌آورید، آن را با
`Tech.escapeHtml` پاک‌سازی کنید یا از `badge()` استفاده کنید.

| Helper | کاربرد |
| --- | --- |
| `formatDate(value, locale = 'fa-IR')` | تاریخ |
| `formatDateTime(value, locale = 'fa-IR')` | تاریخ و ساعت |
| `formatNumber(value, decimals = 0)` | عدد با جداکننده |
| `badge(text, color = 'secondary')` | badge بوت‌استرپ |
| `yesNo(value)` | بله / خیر |
| `boolBadge(value)` | badge فعال / غیرفعال |
| `upper(value)` / `lower(value)` | تبدیل حروف |
| `truncate(value, length = 30)` | کوتاه‌کردن متن |

Helper سفارشی:

```js
Tech.addTemplateHelper('currency', value =>
    Number(value).toLocaleString('fa-IR') + ' تومان');
```

این ثبت باید **بعد از بارگذاری `techui.js`** اجرا شود. اگر کد را داخل یک
View می‌نویسید، آن را در بخش `Scripts` بگذارید (که پس از اسکریپت کتابخانه در
Layout رندر می‌شود) یا در انتهای Layout بعد از تگ
`<script src="~/_content/TechUI.Mvc/js/techui.js"></script>`. قرار دادن آن در
اسکریپتی که زودتر از `techui.js` لود می‌شود (مثلاً `site.js`) خطای
`Tech is not defined` می‌دهد.

---

## رویدادها

`Change("handlerName")` نام یک تابع **سراسری** (`window.handlerName`) را
می‌گیرد. برای کامپوننت‌های ورودی، handler روی wrapper با
`data-event-change` ذخیره می‌شود؛ برای Grid و Tabs/Accordion هم رویداد با
`Tech.trigger` و همان الگو ارسال می‌شود. payload هر رویداد در بخش همان
کامپوننت آمده است.

---

## یکپارچگی EF Core

بسته‌ی `TechUI.Mvc.EntityFrameworkCore` یک متد الحاقی برای اعمال فیلتر،
مرتب‌سازی و صفحه‌بندی روی `IQueryable<T>` ارائه می‌دهد:

```csharp
using TechUI.Mvc.Grid;

[HttpPost]
public async Task<IActionResult> GridData(TechGridRequest request)
{
    var result = await _db.Users
        .AsNoTracking()
        .ToTechGridResultAsync(
            request,
            allowedFields: new[] { "Id", "FullName", "IsActive" });

    return Json(result);
}
```

`allowedFields` لیست سفید فیلدهای قابل فیلتر/مرتب‌سازی است (در صورت
`null` بودن، همه‌ی property‌های عمومی مجاز هستند). صفحه‌بندی بین ۱ و
`TechGridRequest.MaxPageSize` (پیش‌فرض ۱۰۰۰) محدود می‌شود.
