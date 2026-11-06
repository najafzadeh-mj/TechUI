# TechUI.Mvc

Lightweight, dependency-free UI components for **ASP.NET Core MVC 9** with a
fluent server-side builder API (`@Html.Tech()...`), no TagHelpers, and
Persian-first defaults.

- Server-rendered HTML with progressive JavaScript enhancement.
- RTL-ready and built-in dark theme.
- Jalali date/time/range pickers on a shared calendar engine.
- Client + server validation from DataAnnotations and `ModelState`.
- Grid, DropDownList, MultiSelect/Tag Input, inputs, editor, tabs, accordion,
  tree view, dropdown tree, wizard/stepper, timeline, breadcrumb, file upload,
  spinner, toast, dialog and dependency-free SVG charts (23 chart types).

## Requirements

- .NET 9 / ASP.NET Core MVC
- Bootstrap 5.x and Bootstrap Icons

## Install

```bash
dotnet add package TechUI.Mvc
```

## Setup

`Views/_ViewImports.cshtml`:

```razor
@using TechUI.Mvc.Html
@using TechUI.Mvc.Infrastructure
```

`Views/Shared/_Layout.cshtml`:

```html
<link rel="stylesheet" href="~/_content/TechUI.Mvc/css/techui.css" />
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
<script src="~/_content/TechUI.Mvc/js/techui.js"></script>
```

## Usage

```razor
@(Html.Tech().TextBoxFor<MyModel, string?>(m => m.Name)
    .Placeholder("نام")
    .PrefixIcon("person")
    .Clearable())

@(Html.Tech().DatePickerFor<MyModel, DateTime?>(m => m.BirthDate)
    .Max(DateTime.Today))
```

Components boot from `data-tech-component` on `DOMContentLoaded`; call
`Tech.boot()` after injecting markup dynamically.

## Editor media

```razor
@(Html.Tech().EditorFor<MyModel, string?>(m => m.Content)
    .Upload(Url.Action("Upload", "Home"))
    .MediaLibrary(Url.Action("MediaLibrary", "Home"))
    .MaxUploadSize(10 * 1024 * 1024))
```

The upload endpoint receives `multipart/form-data` (file field `file` plus
`kind=image|file`) and returns `{ url, name, size, type }`; the library endpoint
returns an array of the same shape and is called with `?type=image|file`.
Images and files are inserted at the caret as `.tech-editor-image` and
`.tech-editor-file`.

## TreeView with client template

```razor
@(Html.Tech().TreeView("categories")
    .Items(items => items
        .Add("1", "دیجیتال", node => node
            .Expanded()
            .Children(children => children.Add("2", "موبایل"))))
    .ClientTemplate("<b>#: text #</b>"))
```

`#: field #` is HTML-encoded, `#= field #` is raw HTML.

## DropDownTree

```razor
@(Html.Tech().DropDownTreeFor<MyModel, string?>(m => m.Category)
    .Filterable()
    .Items(items => items
        .Add("1", "دیجیتال", node => node
            .Children(children => children.Add("2", "موبایل")))))
```

A `string` expression binds a single value; `List<string>`/`string[]` renders a
multi-select tree with cascading checkboxes and chips.

## Charts

```razor
@using TechUI.Mvc.Chart

@(Html.Tech().Chart("sales")
    .Type(ChartTypes.Column)
    .Categories("فروردین", "اردیبهشت", "خرداد")
    .Series("حضوری", 320, 410, 380)
    .Series("آنلاین", 240, 280, 350)
    .Legend()
    .Tooltip())
```

Charts render as inline SVG on the client with no external dependency. Types
include column/bar, line/spline/step, area/areaspline, pie, donut, polar,
scatter, bubble, radar, gauge, radial bar, heatmap, funnel, treemap,
waterfall, candlestick, bullet, sparkline and spark bar; series can mix types
(e.g. a dashed spline target over columns). The first palette color is
`--tech-primary`, so theme presets recolor charts automatically. Runtime API:

```js
Tech.Chart.get('sales').update({ series: [/* ... */] });
Tech.Chart.render('#dynamicChart');
```

## FileUpload

```razor
@(Html.Tech().FileUpload("attachments")
    .Multiple()
    .Accept(".png", ".jpg", ".pdf", ".zip")
    .MaxSizeMb(10)
    .MaxFiles(5)
    .Url(Url.Action("Upload", "Home"))
    .Events(events => events
        .Success("onFileUploaded")
        .Error("onUploadFailed")))
```

With `Url`, files are posted one by one as `multipart/form-data` (field
`file`, antiforgery token included) and XHR upload progress is shown. A
successful JSON response stores `id`/`value`/`name` in a hidden input named
after the builder. Without `Url`, selections post natively with the form.
Client API: `Tech.FileUpload.get('attachments').upload()`, `.clear()`,
`.getFiles()`.

## Wizard / Stepper

```razor
@(Html.Tech().Wizard("checkoutWizard")
    .Step("Customer", customerStep, "Name and email", "person")
    .Step("Company", companyStep, "Company and plan", "building")
    .Step("Review", "<p>Final check</p>", null, "check2-circle")
    .SubmitOnFinish()
    .Events(e => e.Change("onWizardChange").Finish("onWizardFinish")))
```

Linear (default) or free navigation, horizontal or vertical layout, progress
bar, per-step validation through `Tech.validate` (any `data-val` fields,
including `asp-for` inputs) and `aria-current="step"` markup. Runtime API:

```js
const wizard = Tech.Wizard.get('checkoutWizard');
wizard.next(); wizard.prev(); wizard.goTo(2); wizard.reset();
wizard.state(); // { index, total, last, completed, steps }
```

## Timeline

```razor
@using TechUI.Mvc.Timeline

@(Html.Tech().Timeline("orderTimeline")
    .Item("Delivered", "Today — 11:02", "Handed to the customer.",
        "check2-circle", TimelineTones.Success, "Done")
    .Item("Shipped", "Yesterday — 09:15", null,
        "truck", TimelineTones.Info))
```

Server-rendered `ol/li` timeline with icon or dot markers, time, badge,
tone colors (`primary`, `success`, `info`, `warning`, `danger`, `neutral`),
compact mode, RTL/dark-theme support and an `IHtmlContent` overload for rich
bodies. No JavaScript required.

## Breadcrumb

```razor
@(Html.Tech().Breadcrumb("shopBreadcrumb")
    .Home("/")
    .Item("Shop", "/shop", "shop")
    .Item("Accessories", "/shop/accessories", "headphones")
    .Item("Wireless Headphones", null, "earbuds"))
```

Server-rendered `nav > ol` breadcrumb with linked items, icons, `aria-current`
on the last item, optional custom separator and compact spacing. The separator
flips automatically for RTL and colors follow the active theme. No JavaScript
required.

## Theme and RTL

```js
Tech.theme.toggle();              // 'dark' | 'light', persisted in localStorage
Tech.theme.setPreset('emerald');  // default, emerald, violet, rose, amber, slate
```

Presets change the accent palette and combine with both light and dark modes.
Wrap any region in `.tech-rtl` (or set `dir="rtl"`) to mirror components.

Full fluent and JavaScript API reference:
https://github.com/najafzadeh-mj/TechUI (`docs/components.md`)

Sample applications:

- `src/samples/TechUI.SampleMvc` — component gallery (each component in isolation).
- `src/samples/TechUI.SampleAdmin` — complete Persian admin panel with EF Core
  SQLite, dashboard, server-side grids, validated CRUD, reports and settings.

## License

MIT
