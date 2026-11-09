using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Race.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC + access to HttpContext (needed by ApiClient to read the session)
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// 2. SESSION: stores the logged-in user's ID, role, name and the API cookie.
//    A unique cookie name is REQUIRED: the API and the MVC app both run on
//    "localhost", and browsers share cookies between ports.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Race.MVC.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);   // same as the API
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. TYPED HTTP CLIENT for the Part 2 API.
//    UseCookies = false is essential: the handler is shared by all users, so
//    cookies are attached manually, per user, from the MVC session.
string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is missing.");

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler { UseCookies = false };
    if (builder.Environment.IsDevelopment())
    {
        // Development only: accept the local self-signed API certificate.
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

var app = builder.Build();

// 4. Friendly error pages (no stack traces for users)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/HttpError", "?code={0}");

app.UseStaticFiles();

// 5. Always parse/format numbers with a dot (e.g. 10.5 km), whatever the PC's region is
var culture = CultureInfo.InvariantCulture;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture }
});

app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id:int?}");

app.Run();
