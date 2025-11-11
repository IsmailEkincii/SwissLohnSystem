using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1) Config: Connection string'i appsettings.json'dan oku
var connStr = builder.Configuration.GetConnectionString("Default");

// 2) Controllers
builder.Services.AddControllers();

// 3) DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connStr)
);

// 4) CORS – UI köken(ler)ini ekle
const string CorsPolicy = "UIOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicy, policy =>
    {
        policy
            // Geliþtirmede geçici olarak tamamen açabilirsin:
            //.AllowAnyOrigin()
            // veya sadece UI adreslerine izin ver (önerilen):
            .WithOrigins(
                "https://localhost:7296", // UI HTTPS portun
                "http://localhost:5173",  // Vite/SPA vs. kullanýyorsan
                "http://localhost:5003"   // UI HTTP portun (varsa)
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 5) CORS middleware – MapControllers()'dan ÖNCE!
app.UseCors(CorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
