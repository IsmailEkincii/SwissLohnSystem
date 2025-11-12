// Controllers/SettingsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Setting;


[Route("api/[controller]")]
[ApiController]
// [Authorize(Policy="Settings.Edit")] // istersen aç
public class SettingsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public SettingsController(ApplicationDbContext db) => _db = db;

    // Tümü (opsiyonel prefix filtre)
    // GET /api/Settings?prefix=AHV.   veya  ?prefix=ALV.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SettingDto>>> GetAll([FromQuery] string? prefix = null)
    {
        var q = _db.Settings.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(prefix))
            q = q.Where(s => s.Name.StartsWith(prefix));
        var data = await q
            .OrderBy(s => s.Name)
            .Select(s => new SettingDto { Name = s.Name, Value = s.Value, Description = s.Description })
            .ToListAsync();
        return Ok(data);
    }

    // Tek ayar
    [HttpGet("{name}")]
    public async Task<ActionResult<SettingDto>> GetOne(string name)
    {
        var s = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name);
        if (s is null) return NotFound();
        return Ok(new SettingDto { Name = s.Name, Value = s.Value, Description = s.Description });
    }

    // Tek ayar upsert (PUT /api/Settings/AHV.Employee)
    [HttpPut("{name}")]
    public async Task<IActionResult> UpsertOne(string name, [FromBody] SettingDto dto)
    {
        if (!string.Equals(name, dto.Name, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Name mismatch.");
        if (!IsValidKey(dto.Name)) return BadRequest("Invalid key.");
        if (!IsValidValue(dto.Name, dto.Value)) return BadRequest("Invalid value.");

        var s = await _db.Settings.FirstOrDefaultAsync(x => x.Name == dto.Name);
        if (s is null)
            _db.Settings.Add(new SwissLohnSystem.API.Models.Setting { Name = dto.Name, Value = dto.Value, Description = dto.Description });
        else
        {
            s.Value = dto.Value;
            s.Description = dto.Description;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Toplu güncelle (Dashboard save)
    // PUT /api/Settings (body: SettingDto[])
    [HttpPut]
    public async Task<IActionResult> BulkUpsert([FromBody] IEnumerable<SettingDto> dtos)
    {
        foreach (var dto in dtos)
        {
            if (!IsValidKey(dto.Name)) return BadRequest($"Invalid key: {dto.Name}");
            if (!IsValidValue(dto.Name, dto.Value)) return BadRequest($"Invalid value for {dto.Name}");
            var s = await _db.Settings.FirstOrDefaultAsync(x => x.Name == dto.Name);
            if (s is null)
                _db.Settings.Add(new SwissLohnSystem.API.Models.Setting { Name = dto.Name, Value = dto.Value, Description = dto.Description });
            else
            {
                s.Value = dto.Value;
                s.Description = dto.Description;
            }
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Basit key/value validasyonları
    private static bool IsValidKey(string key) =>
        key.StartsWith("AHV.") || key.StartsWith("ALV.") || key.StartsWith("UVG.") ||
        key.StartsWith("BVG.") || key.StartsWith("FAK.") || key.StartsWith("Rounding.") ||
        key.StartsWith("QST."); // ileride QST tarifleri için

    private static bool IsValidValue(string key, decimal value)
    {
        if (key.StartsWith("Rounding."))
            return value == 0.01m || value == 0.05m; // istersen gevşet
        if (key is "ALV.CapAnnual" or "BVG.EntryThresholdAnnual" or "BVG.CoordinationDedAnnual" or "BVG.UpperLimitAnnual")
            return value >= 0;
        // oranlar
        return value >= 0 && value <= 1.0m || value > 1m; // >1 ise sen % girebilirsin (EfSettingsProvider normalize ediyor)
    }
}
