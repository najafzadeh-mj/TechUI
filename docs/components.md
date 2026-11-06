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
- [Wizard / Stepper](#wizard--stepper)
- [Timeline](#timeline)
- [Breadcrumb](#breadcrumb)
- [Chart](#chart)
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

در `_ViewImports.cshtml`:

```razor
@using TechUI.Mvc.Html
@using TechUI.Mvc.Infrastructure
```

در `_Layout.cshtml`:

```html
<link rel="stylesheet" href="~/_content/TechUI.Mvc/css/techui.css" />
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
    .Filterable()
    .Selectable()
    .Resizable()
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
        .Error("onGridError"))
    .Toolbar(t =>
    {
        t.Create("افزودن", "onCreate", "plus");
        t.Refresh("بازخوانی");
        t.Custom("خروجی اکسل", "onExport", "download");
        t.ColumnChooser();
    })
    .Columns(c =>
    {
        c.Bound(x => x.Id).Title("شناسه").Width(100).Filterable();
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
| `Pageable`, `Sortable`, `Filterable`, `Selectable`, `Resizable` | فعال‌سازی قابلیت‌ها |
| `PageSize(int)` | اندازه‌ی صفحه‌ی اولیه |
| `PageSizeSelector()` / `PageSizes(params int[])` | انتخابگر اندازه‌ی صفحه؛ `PageSizes` انتخابگر را خودکار فعال می‌کند |
| `DataSource(Action<GridDataSourceBuilder>)` | `Read(action, controller, area?)`, `Url(url)`, `Data(functionName)`, `Type(TechHttpMethod)` |
| `Events(Action<GridEventBuilder>)` | `DataBinding`, `DataBound`, `Change`, `Error` |
| `Toolbar(Action<GridToolbarBuilder>)` | `Create(text?, handler?, icon?)`, `Refresh(text?, icon?)`, `Custom(text, handler, icon?)`, `ColumnChooser(text?, icon?)` |
| `Columns(Action<GridColumnFactory<TModel>>)` | `Bound(expression)` و `Template(string)` |

`GridColumnBuilder`: `Title`, `Width`, `Sortable`, `Filterable`.

ستون‌های `Bound` نوع داده را هم برای کلاینت ارسال می‌کنند؛ پاپ‌آپ فیلتر برای
ستون رشته‌ای به‌صورت پیش‌فرض `contains` و برای ستون عددی، تاریخی یا بولین
`eq` را انتخاب می‌کند و فقط عملگرهای سازگار را نشان می‌دهد. `Bound` از
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

`TechGridResult<T>` شامل `Data`, `Total`, `Page`, `PageSize` است. فیلترها در
`Filter` (یک گروه با `Logic` برابر `and`/`or` و لیست `Filters` شامل `Field`,
`Operator`, `Value`) و مرتب‌سازی‌ها در `Sorts` (`Field`, `Dir`) قرار
می‌گیرند. عملگرهای پشتیبانی‌شده:
`contains`, `notcontains`, `startswith`, `endswith`, `eq`, `neq`, `gt`, `gte`,
`lt`, `lte`, `in`, `notin`, `isnull`, `isnotnull`.

### API سمت کلاینت

```js
const grid = Tech.Grid.get('usersGrid');

await grid.refresh();
grid.selectedData();                       // ردیف انتخاب‌شده یا null
grid.clearSelection();
grid.setFilter('FullName', 'علی');         // فقط state را تغییر می‌دهد
grid.clearFilters();
await grid.refresh();                      // برای اعمال فیلترها
grid.state();                              // { page, pageSize, sorts, filters }
```

`setFilter` و `clearFilters` درخواستی ارسال نمی‌کنند؛ بعد از آن‌ها
`refresh()` را صدا بزنید. عملگر پیش‌فرض `setFilter` بر اساس نوع ستون انتخاب
می‌شود و در صورت نیاز می‌توانید عملگر را به‌صورت آرگومان سوم بدهید.

مرتب‌سازی چندستونه با نگه‌داشتن `Shift` و کلیک روی سرستون‌ها انجام می‌شود.
ستون‌های مخفی‌شده و عرض ستون‌ها و اندازه‌ی صفحه در `localStorage` ذخیره
می‌شوند. سرستون‌های قابل مرتب‌سازی و ردیف‌های قابل انتخاب با صفحه‌کلید
(`Enter` / `Space`) هم کار می‌کنند.

عنصر گرید هم `element.techGrid` را در اختیار می‌گذارد. رخدادها با `sender`
(نمونه‌ی گرید) ارسال می‌شوند؛ `DataBound` مقدار `data` را هم دارد.

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

تقویم جلالی است؛ مقدار ارسالی به سرور و مخفی به‌صورت ISO
(`yyyy-MM-dd`) و متن نمایشی با `Format` تولید می‌شود.

```razor
@(Html.Tech().DatePickerFor<MyModel, DateTime?>(m => m.BirthDate)
    .Format("yyyy/MM/dd")
    .Min(new DateTime(1900, 1, 1))
    .Max(DateTime.Today)
    .TodayButton()
    .ClearButton()
    .OpenOnFocus())
```

اختصاصی: `Format(string)`, `Min(DateTime)`, `Max(DateTime)`,
`TodayButton(bool = true)`, `ClearButton(bool = true)`,
`OpenOnFocus(bool = true)`.

```js
const picker = Tech.DatePicker.get('BirthDate');
picker.value();                  // ISO
picker.setValue('1990-05-20');   // ISO میلادی
picker.open();
picker.close();
```

رویداد `Change`: `{ sender, value }` که `value` همان ISO مخفی است.

---

## DateRangePicker

دو فیلد شروع/پایان با یک پنل مشترک (دو ماه نمایش هم‌زمان).

```razor
@(Html.Tech().DateRangeFor<ReportModel, DateTime?, DateTime?>(
        m => m.FromDate, m => m.ToDate)
    .Placeholders("تاریخ شروع", "تاریخ پایان")
    .Min(new DateTime(2020, 1, 1))
    .Max(DateTime.Today)
    .TodayButton()
    .ClearButton())

@* بدون مدل *@
@(Html.Tech().DateRange("ReportFrom", "ReportTo")
    .Values("2026-10-01", "2026-10-31")
    .TodayButton(false))
```

| متد | توضیح |
| --- | --- |
| `Start(object?)` / `End(object?)` / `Values(start, end)` | مقدار اولیه |
| `Placeholders(start, end)` | متن راهنمای دو فیلد |
| `Format(string)` | قالب نمایشی جلالی |
| `Min(DateTime)` / `Max(DateTime)` | محدوده‌ی مجاز |
| `TodayButton` / `ClearButton` / `OpenOnFocus` | کنترل دکمه‌ها و رفتار باز شدن |

هر دو فیلد مخفی با نام‌های مشخص‌شده به سرور ارسال می‌شوند (ISO میلادی).
اعتبارسنجی روی فیلد شروع اعمال می‌شود.

```js
const range = Tech.DateRangePicker.get('ReportFrom');

range.value();                              // { start, end }
range.setValue('2026-10-01', '2026-10-31');
range.open();
range.close();
```

رویداد `Change`: `{ sender, start, end }`. اگر کاربر ابتدا پایان را انتخاب کند،
خودکار به شروع تبدیل می‌شود و تاریخ‌های معکوس جابه‌جا می‌شوند.

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
