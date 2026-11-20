using Microsoft.EntityFrameworkCore;
using TechUI.Mvc.Captcha;
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
builder.Services.AddSingleton<ProfileService>();

// Captcha tokens are encrypted with this secret. The sample falls back to a
// random per-process secret, so restarting the app invalidates open challenges.
builder.Services.AddTechUICaptcha(options =>
{
    options.Secret = builder.Configuration["TechUI:Captcha:Secret"] ?? "";
    options.ExpiresIn = TimeSpan.FromMinutes(10);
});

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
app.MapTechUICaptcha();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();
