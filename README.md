# TechUI.Mvc

Lightweight UI components for **ASP.NET Core MVC 9** with no JavaScript
dependencies, a fluent server-side builder API (`@Html.Tech()...`), no
TagHelpers, and Persian-first defaults.

- Server-rendered HTML with progressive JavaScript enhancement.
- RTL-ready (`.tech-rtl` wrapper) and dark theme (`data-tech-theme="dark"`).
- Jalali (Persian) date and time pickers built on a shared calendar engine.
- Form validation that understands DataAnnotations, `ModelState` and Persian messages.
- Optional Entity Framework Core integration for grid filtering, sorting and paging.

## Requirements

- .NET 9 / ASP.NET Core MVC
- Bootstrap 5.3 CSS (form styling, badges and utilities) and Bootstrap Icons;
  both are peer assets and are not bundled with the package
- A modern browser

## Packages

| Package | Purpose |
| --- | --- |
| `TechUI.Mvc` | Components, JavaScript and CSS |
| `TechUI.Mvc.EntityFrameworkCore` | `ToTechGridResultAsync` for EF Core grids |

```bash
dotnet add package TechUI.Mvc
dotnet add package TechUI.Mvc.EntityFrameworkCore
```

## Setup

1. Add TechUI namespaces to `Views/_ViewImports.cshtml`:

```razor
@using TechUI.Mvc.Html
@using TechUI.Mvc.Infrastructure
```

2. Include the stylesheet and script in `Views/Shared/_Layout.cshtml`
   (RCL static assets are served from `_content`):

```html
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/_content/TechUI.Mvc/css/techui.css" />
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
...
<script src="~/_content/TechUI.Mvc/js/techui.js"></script>
```

Load Bootstrap CSS before `techui.css` so the component overrides win. TechUI
does not need Bootstrap's JavaScript; load `bootstrap.bundle.min.js` only if you
use Bootstrap's own interactive components.

3. Render any component with the `Tech()` helper:

```razor
@(Html.Tech().TextBoxFor<MyModel, string?>(m => m.Name)
    .Placeholder("نام")
    .PrefixIcon("person")
    .Clearable())
```

Components boot automatically on `DOMContentLoaded` by scanning
`data-tech-component` attributes. Call `Tech.boot()` manually if you inject
markup after load.

## RTL

Wrap a region in `.tech-rtl` (or set `dir="rtl"` on the page). Components use
logical CSS properties, so they mirror without duplicating styles:

```html
<div class="tech-rtl">
    @(Html.Tech().DatePicker("BirthDate"))
</div>
```

## Dark theme and color presets

`Tech.theme` toggles `data-tech-theme` on `<html>` and remembers the choice in
`localStorage`. The CSS variables and component overrides are built in:

```js
Tech.theme.set('dark');
Tech.theme.toggle();
Tech.theme.get(); // 'dark' | 'light'
```

Five accent presets are available in addition to the default palette — they
work in both light and dark mode:

```js
Tech.theme.setPreset('emerald'); // default, emerald, violet, rose, amber, slate
Tech.theme.getPreset();
```

You can also apply the theme statically with `<html data-tech-theme="dark">`
or to a single wrapper with `<div class="tech-dark">`, and the palette with
`data-tech-preset="emerald"` or `<div class="tech-preset-emerald">`.

## Client validation

Server components emit `data-val*` attributes from DataAnnotations and
`ModelState`. `Tech.boot()` wires forms automatically (`novalidate`, submit
blocking, live re-validation) and displays Persian messages. Composite controls
(date/time/range pickers, dropdowns, multiselect, checkbox/radio) are validated
through their hidden inputs, so `ModelState` and client rules stay in sync.

```razor
@(Html.Tech().TextBoxFor<MyModel, string?>(m => m.Email)
    .ValidationMessage())
```

## Editor media

The editor toolbar can upload or pick images and files from a media library:

```razor
@(Html.Tech().EditorFor<MyModel, string?>(m => m.Content)
    .Upload(Url.Action("Upload", "Home"))
    .MediaLibrary(Url.Action("MediaLibrary", "Home"))
    .MaxUploadSize(10 * 1024 * 1024))
```

Uploads are `multipart/form-data` POSTs returning `{ url, name, size, type }`;
the library is a GET endpoint called with `?type=image|file`.

## Components

Grid, DropDownList, TextBox, TextArea, NumericTextBox, MaskedTextBox, Editor,
CheckBox, RadioButton, DatePicker, DateRangePicker, TimePicker, MultiSelect /
Tag Input, Tabs, Accordion, TreeView, DropDownTree, FileUpload, Captcha,
Wizard / Stepper, Timeline, Breadcrumb, Chat, Chart (23 types), Spinner, Toast
and Dialog. The
standalone Chat panel works as an AI assistant or a human support inbox, and the
Grid also offers an optional assistant chat panel backed by your own endpoint —
TechUI ships no AI dependency; see
[Grid Chat](docs/components.md#چت-دستیار-grid-chat). The gallery sample wires its
AI chat to DeepSeek's OpenAI-compatible API when `DEEPSEEK_API_KEY` (or
`DeepSeek:ApiKey`) is set and falls back to a local responder otherwise; see
[Chat](docs/components.md#chat).

Full fluent API and JavaScript API reference:
[docs/components.md](docs/components.md).

A runnable sample for every component lives in `src/samples/TechUI.SampleMvc`.
A complete Persian admin panel — SQLite + EF Core, dashboard, server-side grids,
validated CRUD, reports and settings — lives in `src/samples/TechUI.SampleAdmin`.

Build and run from the repository root:

```bash
dotnet build TechUI.sln
dotnet run --project src/samples/TechUI.SampleMvc
dotnet run --project src/samples/TechUI.SampleAdmin
```

## License

[MIT](LICENSE) © Majid Najafzadeh
