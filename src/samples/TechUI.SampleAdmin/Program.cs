using Microsoft.EntityFrameworkCore;
using TechUI.SampleAdmin.Data;
using TechUI.SampleAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

// Serve referenced-project static assets (_content/...) in every environment,
// not only Development, so the sample also runs from the build output.
builder.WebHost.UseStaticWebAssets();

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "App_Data"));

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=App_Data/techui-admin.db"));

builder.Services.AddSingleton<SettingsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdminDbContext>();

    SeedData.Initialize(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Admin/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
