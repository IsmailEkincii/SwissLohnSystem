using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;

var builder = WebApplication.CreateBuilder(args);

// DbContext ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware ve routing
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
