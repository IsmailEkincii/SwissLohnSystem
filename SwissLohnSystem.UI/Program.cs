using SwissLohnSystem.UI.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Api base url (tek yerden okunacak)
var baseUrl =
    builder.Configuration["ApiBaseUrl"]
    ?? builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Missing Api base URL. Set Api:BaseUrl (or ApiBaseUrl).");

builder.Services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri(baseUrl.TrimEnd('/'));
});

// MVC (Controllers + Views) kullanýyorsan bunu aç
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Razor Pages kullanýyorsan kalsýn
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files cache ayarý (dev’de no-cache)
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    });
}
else
{
    app.UseStaticFiles();
}

app.UseRouting();

// Eðer auth yoksa þimdilik gerek yok; ileride eklersin:
// app.UseAuthentication();
// app.UseAuthorization();

// Controllers (MVC) route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API Controller’larýn attribute routing ile çalýþmasý için
app.MapControllers();

// Razor Pages
app.MapRazorPages();

app.Run();
