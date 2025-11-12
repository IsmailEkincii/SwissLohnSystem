using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models; // ?? eklendi
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Services.Payroll;

var builder = WebApplication.CreateBuilder(args);

// DI
builder.Services.AddScoped<ISettingsProvider, EfSettingsProvider>();
builder.Services.AddScoped<IPayrollCalculator, PayrollCalculator>();

// Connection string
var connStr =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    builder.Configuration.GetConnectionString("Default") ??
    throw new InvalidOperationException("Connection string not found.");

// Controllers + JSON (DateOnly/TimeOnly)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        o.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());  // ?? eklendi
        o.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());  // ?? eklendi
    });

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connStr));

// CORS
const string CorsPolicy = "AllowUI";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins("https://localhost:7296", "http://localhost:7296")
        .AllowAnyHeader()
        .AllowAnyMethod()
    );
});

// Swagger + DateOnly/TimeOnly eþlemeleri
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SwissLohnSystem API", Version = "v1" });
    // ?? Swagger þema üretimi için gerekli map’ler
    c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

    // Çakýþan route varsa (ayný method+path), en azýndan build etsin:
    // c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First()); // gerekirse aç
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();
app.Run();


// ---------------- helper converters ----------------
public sealed class DateOnlyJsonConverter : System.Text.Json.Serialization.JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";
    public override DateOnly Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString()!);
    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateOnly value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}

public sealed class TimeOnlyJsonConverter : System.Text.Json.Serialization.JsonConverter<TimeOnly>
{
    private const string Format = @"HH\:mm\:ss";
    public override TimeOnly Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        => TimeOnly.Parse(reader.GetString()!);
    public override void Write(System.Text.Json.Utf8JsonWriter writer, TimeOnly value, System.Text.Json.JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}
