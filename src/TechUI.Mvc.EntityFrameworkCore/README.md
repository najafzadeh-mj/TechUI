# TechUI.Mvc.EntityFrameworkCore

Entity Framework Core integration for **TechUI.Mvc** grids: applies TechUI
filtering, sorting and paging to an `IQueryable<T>` and returns a
`TechGridResult<T>` ready for `Json(...)`.

## Install

```bash
dotnet add package TechUI.Mvc
dotnet add package TechUI.Mvc.EntityFrameworkCore
```

## Usage

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

`allowedFields` is an optional whitelist of properties clients may sort or
filter by; when omitted, any public property is allowed.

Supported filter operators: `contains`, `notcontains`, `startswith`,
`endswith`, `eq`, `neq`, `gt`, `gte`, `lt`, `lte`, `in`, `notin`, `isnull`,
`isnotnull`. Paging is clamped to `TechGridRequest.MaxPageSize` (1000).

## Requirements

- .NET 9
- `TechUI.Mvc` 1.0.0
- EF Core 9

## License

MIT
