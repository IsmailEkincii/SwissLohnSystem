using SwissLohnSystem.UI.Services;
using Microsoft.AspNetCore.StaticFiles;


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
app.MapRazorPages();
app.Run();
