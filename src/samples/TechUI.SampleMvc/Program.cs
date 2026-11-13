using TechUI.Mvc.Captcha;
using TechUI.SampleMvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Serve referenced-project static assets (_content/...) in every environment,
// not only Development, so the sample also runs from the build output.
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<SupportChatStore>();

// DeepSeek is optional: without a key the sample falls back to LocalAssistant.
// Provide the key with user-secrets ("DeepSeek:ApiKey") or DEEPSEEK_API_KEY.
builder.Services
    .AddOptions<DeepSeekOptions>()
    .Bind(builder.Configuration.GetSection("DeepSeek"))
    .PostConfigure(options =>
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            options.ApiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
        }
    });

builder.Services.AddHttpClient<DeepSeekAssistant>();

// Captcha tokens are encrypted with this secret. The sample falls back to a
// random per-process secret, so restarting the app invalidates open challenges.
builder.Services.AddTechUICaptcha(options =>
{
    options.Secret = builder.Configuration["TechUI:Captcha:Secret"] ?? "";
    options.ExpiresIn = TimeSpan.FromMinutes(10);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapTechUICaptcha();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
