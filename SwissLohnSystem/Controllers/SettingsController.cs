// Controllers/SettingsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Setting;

[Route("api/[controller]")]
[ApiController]
public class SettingsController : ControllerBase
{
    private static readonly string[] AllowedPrefixes = { "AHV.", "ALV.", "UVG.", "BVG.", "FAK.", "Rounding.", "QST." };

    private readonly ApplicationDbContext _db;
    public SettingsController(ApplicationDbContext db) => _db = db;

    // GET /api/Settings?prefix=AHV.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SettingDto>>> GetAll([FromQuery] string? prefix = null, CancellationToken ct = default)
    {
        var q = _db.Settings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            var p = prefix.Trim();
            q = q.Where(s => s.Name.StartsWith(p));
        }

        var data = await q
     .OrderBy(s => s.Name)
     .Select(s => new SettingDto { Id = s.Id, Name = s.Name, Value = s.Value, Description = s.Description })
     .ToListAsync(ct);

        return Ok(data);
    }

    // GET /api/Settings/{name}
    [HttpGet("{name}")]
    public async Task<ActionResult<SettingDto>> GetOne(string name, CancellationToken ct = default)
    {
        var key = name.Trim();
        var s = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Name == key, ct);
        if (s is null) return NotFound();
        return Ok(new SettingDto { Id = s.Id, Name = s.Name, Value = s.Value, Description = s.Description });
    }

    // PUT /api/Settings/{name}   (single upsert)
    [HttpPut("{name}")]
    public async Task<IActionResult> UpsertOne(string name, [FromBody] SettingDto dto, CancellationToken ct = default)
    {
        if (dto is null) return BadRequest("Body required.");
        var key = (dto.Name ?? "").Trim();
        if (!string.Equals(name.Trim(), key, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Name mismatch.");
        if (!IsValidKey(key)) return BadRequest("Invalid key.");
        if (!IsValidValue(key, dto.Value)) return BadRequest("Invalid value.");

        var s = await _db.Settings.FirstOrDefaultAsync(x => x.Name == key, ct);
        if (s is null)
            _db.Settings.Add(new SwissLohnSystem.API.Models.Setting { Name = key, Value = dto.Value, Description = Clean(dto.Description) });
        else
        {
            s.Value = dto.Value;
            s.Description = Clean(dto.Description);
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // PUT /api/Settings   (bulk upsert)
    [HttpPut]
    public async Task<IActionResult> BulkUpsert([FromBody] IEnumerable<SettingDto> dtos, CancellationToken ct = default)
    {
        if (dtos is null) return BadRequest("Body required.");

        var list = dtos
            .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.Name))
            .Select(d => new SettingDto { Name = d.Name!.Trim(), Value = d.Value, Description = Clean(d.Description) })
            .ToList();

        if (list.Count == 0) return BadRequest("No items.");

        // validate all first
        foreach (var d in list)
        {
            if (!IsValidKey(d.Name)) return BadRequest($"Invalid key: {d.Name}");
            if (!IsValidValue(d.Name, d.Value)) return BadRequest($"Invalid value for {d.Name}");
        }

        // fetch all existing in one shot (no N+1)
        var names = list.Select(d => d.Name).Distinct().ToList();
        var existing = await _db.Settings.Where(s => names.Contains(s.Name)).ToListAsync(ct);
        var map = existing.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var d in list)
        {
            if (map.TryGetValue(d.Name, out var s))
            {
                s.Value = d.Value;
                s.Description = d.Description;
            }
            else
            {
                _db.Settings.Add(new SwissLohnSystem.API.Models.Setting
                {
                    Name = d.Name,
                    Value = d.Value,
                    Description = d.Description
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // --- helpers ---
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static bool IsValidKey(string key) => AllowedPrefixes.Any(p => key.StartsWith(p, StringComparison.Ordinal));

    private static bool IsValidValue(string key, decimal value)
    {
        if (key.StartsWith("Rounding.", StringComparison.Ordinal))
            return value == 0.01m || value == 0.05m;

        if (key is "ALV.CapAnnual" or "BVG.EntryThresholdAnnual" or "BVG.CoordinationDedAnnual" or "BVG.UpperLimitAnnual")
            return value >= 0;

        // oranlar: 0..1 (ondalık) veya >1 ise % gibi kullanılabilir (normalize eden provider'da ele alınır)
        return value >= 0;
    }
}
