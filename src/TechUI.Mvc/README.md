# TechUI.Mvc

Lightweight UI components for **ASP.NET Core MVC 9** with no JavaScript
dependencies, a fluent server-side builder API (`@Html.Tech()...`), no
TagHelpers, and Persian-first defaults.

- Server-rendered HTML with progressive JavaScript enhancement.
- RTL-ready and built-in dark theme.
- Jalali date/time/range pickers on a shared calendar engine.
- Client + server validation from DataAnnotations and `ModelState`.
- Grid, DropDownList, MultiSelect/Tag Input, inputs, editor, tabs, accordion,
  tree view, dropdown tree, wizard/stepper, timeline, breadcrumb, file upload,
  captcha, spinner, toast, dialog, dependency-free SVG charts (23 chart
  types) and a dependency-free OpenStreetMap map with markers, popups, shapes,
  layers, search and geolocation.
- Standalone Chat panel with assistant (single request/reply) and support
  (deliver + poll or realtime push) modes; styled by the active theme tokens.
- Optional Grid assistant chat panel with a provider-agnostic request/response
  contract; bring your own endpoint or model, TechUI ships no AI dependency.

## Requirements

- .NET 9 / ASP.NET Core MVC
- Bootstrap 5.3 CSS and Bootstrap Icons (peer assets, not bundled)

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
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/_content/TechUI.Mvc/css/techui.css" />
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
<script src="~/_content/TechUI.Mvc/js/techui.js"></script>
```

Load Bootstrap CSS before `techui.css` so the component overrides win. TechUI
does not need Bootstrap's JavaScript; load `bootstrap.bundle.min.js` only if you
use Bootstrap's own interactive components.

## Usage

```razor
@(Html.Tech().TextBoxFor<MyModel, string?>(m => m.Name)
    .Placeholder("نام")
    .PrefixIcon("person")
    .Clearable())

@(Html.Tech().DatePickerFor<MyModel, DateTime?>(m => m.BirthDate)
    .Max(DateTime.Today))
```

Users can switch the DatePicker between the Jalali and Gregorian month views
with the small toggle inside the panel. Pick the allowed calendars at
definition time with `Calendar(DatePickerCalendar.Jalali)`, `.Gregorian` or
`.Both` (default); fixed modes hide the toggle. The hidden form input,
`Tech.DatePicker.value()` and the `Change` payload always stay Gregorian ISO
(`yyyy-MM-dd`), so server model binding needs no conversion. `DateRangePicker`
accepts the same `Calendar(...)` option and keeps both range hidden inputs in
Gregorian ISO.

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

## Map

```razor
@using TechUI.Mvc.Map

@(Html.Tech().Map("tehran")
    .Height(480)
    .Center(35.71, 51.39)
    .Zoom(12)
    .Marker(35.6997, 51.3380, "میدان آزادی", "<b>میدان آزادی</b>", "building")
    .Marker(35.7522, 51.3756, "برج میلاد", "بلندترین برج ایران", "broadcast", MapTones.Info)
    .Layer(MapSources.OpenStreetMap, selected: true)
    .Layer(MapSources.CartoLight)
    .Layer(MapSources.EsriSatellite)
    .CoordinatesControl())
```

An OpenStreetMap slippy map with no Leaflet/MapLibre dependency: drag, wheel and
pinch zoom, keyboard pan/zoom, markers with HTML popups and dragging, polylines,
polygons, circles by meters, multiple base layers, Nominatim search,
geolocation, fullscreen, scale and coordinate readouts. Built-in providers live
in `MapSources` (`OpenStreetMap`, `CartoLight`, `CartoDark`, `OpenTopoMap`,
`EsriSatellite`); use `.Source(MapSources.X)` for a single provider or add them
as switchable `.Layer(...)` entries. Tiles load over the network; turn on
`Search(...)` to show the search box. Runtime API:

```js
const map = Tech.Map.get('tehran');
map.setView({ lat: 35.7, lng: 51.4 }, 14, true);
const pin = map.addMarker({ lat: 35.7, lng: 51.4, title: 'نقطه', popup: 'توضیح' });
pin.remove();
map.fitBounds([[25.05, 44.03], [39.78, 63.33]]);
map.on('zoomEnd', handler);
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

## Captcha

```csharp
// Program.cs
builder.Services.AddTechUICaptcha(options =>
{
    options.Secret = builder.Configuration["TechUI:Captcha:Secret"] ?? "";
});

app.MapTechUICaptcha(); // GET /techui/captcha
```

```razor
@using TechUI.Mvc.Captcha

@(Html.Tech().Captcha("Captcha")
    .Language(CaptchaLanguage.Persian)
    .Kind(CaptchaKind.Letters)
    .Noise(CaptchaNoiseLevel.Medium)
    .Length(5)
    .Required())
```

Server-generated SVG images with an AES-GCM encrypted token, so no session
state is needed. Kinds are letters, digits or arithmetic
(`CaptchaMathOperation.Addition|Subtraction|Multiplication`), in Persian or
English. Noise is configurable per instance (`CaptchaNoiseLevel.None|Low|Medium|High`)
or globally through `TechCaptchaOptions.Noise` (default `Medium`). Glyphs are
drawn individually with random vertical offsets, rotation and size, and higher
noise levels add more lines, curves and dots. The refresh button pulls a new
challenge from `MapTechUICaptcha`;
validate posts with `await Request.ValidateTechCaptchaAsync(nameof(model.Captcha))`.
Set a stable `Secret` or outstanding tokens die on restart. Client API:
`Tech.Captcha.get('Captcha').refresh()`, `.token()`.

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

## Chat

```razor
@using TechUI.Mvc.Chat

@Html.AntiForgeryToken()

@(Html.Tech().Chat("assistantChat")
    .Mode(TechChatMode.Assistant)
    .Url(Url.Action("ChatAssistant", "Home")!)
    .Title("Assistant")
    .Welcome("How can I help?")
    .Suggestions("Track my order", "Return policy"))
```

The endpoint receives `TechChatRequest` as JSON and returns
`TechChatResponse` with `reply`. With `Mode(TechChatMode.Support)` the same
POST delivers the message to an inbox and agent replies arrive from `PollUrl`
(`?conversation=…&after=<lastId>`) or from
`Tech.Chat.get('assistantChat').receive({ role: 'agent', content: '…' })`
(e.g. SignalR). Polling pauses while the tab is hidden. All colors come from
the theme tokens, so presets and dark mode restyle the panel automatically.

The gallery sample connects its assistant to DeepSeek's OpenAI-compatible
endpoint when `DeepSeek:ApiKey` (user-secrets) or `DEEPSEEK_API_KEY` is set,
and falls back to a local responder otherwise. See
`src/samples/TechUI.SampleMvc/Services/DeepSeekAssistant.cs`.

## Theme and RTL

```js
Tech.theme.toggle();              // 'dark' | 'light', persisted in localStorage
Tech.theme.setPreset('emerald');  // default, emerald, violet, rose, amber, slate
```

Presets change the accent palette and combine with both light and dark modes.
Wrap any region in `.tech-rtl` (or set `dir="rtl"`) to mirror components.

Full fluent and JavaScript API reference:
https://github.com/najafzadeh-mj/TechUI (`docs/components.md`)

Sample applications (run from the repository root):

- `src/samples/TechUI.SampleMvc` — component gallery (each component in isolation):
  `dotnet run --project src/samples/TechUI.SampleMvc`
- `src/samples/TechUI.SampleAdmin` — complete Persian admin panel with EF Core
  SQLite, dashboard, server-side grids, validated CRUD, reports, a customer map
  and settings: `dotnet run --project src/samples/TechUI.SampleAdmin`

## License

[MIT](https://github.com/najafzadeh-mj/TechUI/blob/master/LICENSE)
