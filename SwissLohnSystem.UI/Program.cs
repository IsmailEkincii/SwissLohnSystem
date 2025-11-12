using SwissLohnSystem.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<ApiClient>(c =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"]
                  ?? builder.Configuration["Api:BaseUrl"]
                  ?? throw new InvalidOperationException("Missing ApiBaseUrl");
    c.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddRazorPages();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
