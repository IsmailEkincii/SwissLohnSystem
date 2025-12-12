using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Qst;
using SwissLohnSystem.API.DTOs.Setting;
using SwissLohnSystem.API.Models;
using System.Globalization;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class SettingsController : ControllerBase
{
    private static readonly string[] AllowedPrefixes = { "AHV.", "ALV.", "UVG.", "BVG.", "FAK.", "Rounding.", "QST." };

    private readonly ApplicationDbContext _db;
    public SettingsController(ApplicationDbContext db) => _db = db;

    // ======================
    // SETTINGS
    // ======================

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SettingDto>>> GetAll(
        [FromQuery] string? prefix = null,
        CancellationToken ct = default)
    {
        var q = _db.Settings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            var p = prefix.Trim();
            q = q.Where(s => s.Name.StartsWith(p));
        }

        var data = await q
            .OrderBy(s => s.Name)
            .Select(s => new SettingDto
            {
                Id = s.Id,
                Name = s.Name,
                Value = s.Value,
                Description = s.Description
            })
            .ToListAsync(ct);

        return Ok(data);
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<SettingDto>> GetOne(string name, CancellationToken ct = default)
    {
        var key = name.Trim();
        var s = await _db.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Name == key, ct);
        if (s is null) return NotFound();

        return Ok(new SettingDto
        {
            Id = s.Id,
            Name = s.Name,
            Value = s.Value,
            Description = s.Description
        });
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> UpsertOne(
        string name,
        [FromBody] SettingDto dto,
        CancellationToken ct = default)
    {
        if (dto is null) return BadRequest("Body required.");

        var key = (dto.Name ?? "").Trim();
        if (!string.Equals(name.Trim(), key, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Name mismatch.");
        if (!IsValidKey(key)) return BadRequest("Invalid key.");
        if (!IsValidValue(key, dto.Value)) return BadRequest("Invalid value.");

        var s = await _db.Settings.FirstOrDefaultAsync(x => x.Name == key, ct);
        if (s is null)
        {
            _db.Settings.Add(new Setting
            {
                Name = key,
                Value = dto.Value,
                Description = Clean(dto.Description)
            });
        }
        else
        {
            s.Value = dto.Value;
            s.Description = Clean(dto.Description);
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> BulkUpsert(
        [FromBody] IEnumerable<SettingDto> dtos,
        CancellationToken ct = default)
    {
        if (dtos is null) return BadRequest("Body required.");

        var list = dtos
            .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.Name))
            .Select(d => new SettingDto
            {
                Name = d.Name!.Trim(),
                Value = d.Value,
                Description = Clean(d.Description)
            })
            .ToList();

        if (list.Count == 0) return BadRequest("No items.");

        foreach (var d in list)
        {
            if (!IsValidKey(d.Name)) return BadRequest($"Invalid key: {d.Name}");
            if (!IsValidValue(d.Name, d.Value)) return BadRequest($"Invalid value for {d.Name}");
        }

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
                _db.Settings.Add(new Setting
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

    // ============================
    // QST Tariffs
    // ============================

    [HttpGet("qst-tariffs")]
    public async Task<ActionResult<IEnumerable<QstTariffDto>>> GetQstTariffs(
        [FromQuery] string? canton = null,
        [FromQuery] string? code = null,
        [FromQuery] string? permit = null,
        [FromQuery] bool? church = null,
        CancellationToken ct = default)
    {
        var q = _db.QstTariffs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(canton))
        {
            var c = canton.Trim().ToUpperInvariant();
            q = q.Where(t => t.Canton == c);
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            var c = code.Trim().ToUpperInvariant();
            q = q.Where(t => t.Code == c);
        }

        if (!string.IsNullOrWhiteSpace(permit))
        {
            var p = permit.Trim().ToUpperInvariant();
            q = q.Where(t => t.PermitType == p);
        }

        if (church.HasValue)
        {
            q = q.Where(t => t.ChurchMember == church.Value);
        }

        var list = await q
            .OrderBy(t => t.Canton)
            .ThenBy(t => t.Code)
            .ThenBy(t => t.PermitType)
            .ThenBy(t => t.ChurchMember)
            .ThenBy(t => t.IncomeFrom)
            .Select(t => new QstTariffDto
            {
                Id = t.Id,
                Canton = t.Canton,
                Code = t.Code,
                PermitType = t.PermitType,
                ChurchMember = t.ChurchMember,
                IncomeFrom = t.IncomeFrom,
                IncomeTo = t.IncomeTo,
                Rate = t.Rate,
                Remark = t.Remark
            })
            .ToListAsync(ct);

        return Ok(list);
    }

    [HttpPost("qst-tariffs")]
    public async Task<IActionResult> UpsertQstTariff(
        [FromBody] QstTariffDto dto,
        CancellationToken ct = default)
    {
        if (dto is null)
            return BadRequest("Body required.");

        dto.Canton = (dto.Canton ?? "").Trim().ToUpperInvariant();
        dto.Code = (dto.Code ?? "").Trim().ToUpperInvariant();
        dto.PermitType = (dto.PermitType ?? "").Trim().ToUpperInvariant();
        dto.Remark = string.IsNullOrWhiteSpace(dto.Remark) ? null : dto.Remark.Trim();

        if (string.IsNullOrWhiteSpace(dto.Canton)
            || string.IsNullOrWhiteSpace(dto.Code)
            || string.IsNullOrWhiteSpace(dto.PermitType))
        {
            return BadRequest("Canton, Code und PermitType sind erforderlich.");
        }

        if (dto.IncomeFrom < 0 || dto.IncomeTo < dto.IncomeFrom)
            return BadRequest("Ungültiger Einkommensbereich (IncomeFrom/IncomeTo).");

        if (dto.Rate < 0)
            return BadRequest("Rate darf nicht negativ sein.");

        QstTariff entity;

        if (dto.Id > 0)
        {
            entity = await _db.QstTariffs.FirstOrDefaultAsync(t => t.Id == dto.Id, ct);
            if (entity is null)
                return NotFound($"QST-Tarif mit Id={dto.Id} wurde nicht gefunden.");
        }
        else
        {
            entity = new QstTariff();
            _db.QstTariffs.Add(entity);
        }

        entity.Canton = dto.Canton;
        entity.Code = dto.Code;
        entity.PermitType = dto.PermitType;
        entity.ChurchMember = dto.ChurchMember;
        entity.IncomeFrom = dto.IncomeFrom;
        entity.IncomeTo = dto.IncomeTo;
        entity.Rate = dto.Rate;
        entity.Remark = dto.Remark;

        await _db.SaveChangesAsync(ct);

        dto.Id = entity.Id;
        return Ok(dto);
    }

    [HttpPut("qst-tariffs")]
    public async Task<IActionResult> BulkUpsertQstTariffs(
        [FromBody] IEnumerable<QstTariffDto> dtos,
        CancellationToken ct = default)
    {
        if (dtos is null)
            return BadRequest("Body required.");

        var list = dtos
            .Where(d => d is not null)
            .Select(d =>
            {
                d.Canton = (d.Canton ?? "").Trim().ToUpperInvariant();
                d.Code = (d.Code ?? "").Trim().ToUpperInvariant();
                d.PermitType = (d.PermitType ?? "").Trim().ToUpperInvariant();
                d.Remark = string.IsNullOrWhiteSpace(d.Remark) ? null : d.Remark.Trim();
                return d;
            })
            .Where(d =>
                !string.IsNullOrWhiteSpace(d.Canton) &&
                !string.IsNullOrWhiteSpace(d.Code) &&
                !string.IsNullOrWhiteSpace(d.PermitType))
            .ToList();

        if (list.Count == 0)
            return BadRequest("No items.");

        foreach (var d in list)
        {
            if (d.IncomeFrom < 0 || d.IncomeTo < d.IncomeFrom)
                return BadRequest($"Ungültiger Einkommensbereich für {d.Canton} {d.Code}.");

            if (d.Rate < 0)
                return BadRequest($"Rate darf nicht negativ sein ({d.Canton} {d.Code}).");
        }

        var ids = list.Where(d => d.Id > 0).Select(d => d.Id).ToList();
        var existing = await _db.QstTariffs
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(ct);

        var mapById = existing.ToDictionary(t => t.Id);

        foreach (var d in list)
        {
            if (d.Id > 0 && mapById.TryGetValue(d.Id, out var e))
            {
                e.Canton = d.Canton;
                e.Code = d.Code;
                e.PermitType = d.PermitType;
                e.ChurchMember = d.ChurchMember;
                e.IncomeFrom = d.IncomeFrom;
                e.IncomeTo = d.IncomeTo;
                e.Rate = d.Rate;
                e.Remark = d.Remark;
            }
            else
            {
                var newEntity = new QstTariff
                {
                    Canton = d.Canton!,
                    Code = d.Code!,
                    PermitType = d.PermitType!,
                    ChurchMember = d.ChurchMember,
                    IncomeFrom = d.IncomeFrom,
                    IncomeTo = d.IncomeTo,
                    Rate = d.Rate,
                    Remark = d.Remark
                };
                _db.QstTariffs.Add(newEntity);
            }
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ✅ Swagger ile uyumlu multipart/form-data model
    public sealed class QstTariffImportForm
    {
        public IFormFile? File { get; set; }
    }

    // POST /api/Settings/qst-tariffs/import
    [HttpPost("qst-tariffs/import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> ImportQstTariffsCsv(
        [FromForm] QstTariffImportForm form,
        CancellationToken ct = default)
    {
        var file = form?.File;

        if (file is null || file.Length == 0)
            return BadRequest("CSV file required.");

        string csv;
        using (var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            csv = await sr.ReadToEndAsync(ct);

        var lines = csv
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();

        if (lines.Count == 0)
            return BadRequest("CSV is empty.");

        var sample = lines[0];
        var delim = sample.Contains(';') ? ';' : ',';

        bool IsHeader(string ln)
        {
            var l = ln.ToLowerInvariant();
            return l.Contains("canton") || l.Contains("permit") || l.Contains("income") || l.Contains("church") || l.Contains("rate");
        }

        int start = IsHeader(lines[0]) ? 1 : 0;
        if (start >= lines.Count)
            return BadRequest("CSV has only header.");

        var parsed = new List<QstTariffDto>();
        for (int i = start; i < lines.Count; i++)
        {
            var ln = lines[i];
            var cols = SplitCsvLine(ln, delim);

            if (cols.Count < 7)
                return BadRequest($"Invalid CSV row at line {i + 1}: expected >= 7 columns.");

            var dto = new QstTariffDto();

            dto.Canton = (cols[0] ?? "").Trim().ToUpperInvariant();
            dto.Code = (cols[1] ?? "").Trim().ToUpperInvariant();
            dto.PermitType = (cols[2] ?? "").Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(dto.Canton) ||
                string.IsNullOrWhiteSpace(dto.Code) ||
                string.IsNullOrWhiteSpace(dto.PermitType))
            {
                return BadRequest($"Invalid CSV row at line {i + 1}: Canton/Code/PermitType required.");
            }

            dto.ChurchMember = ParseBool(cols[3]);

            dto.IncomeFrom = ParseDecimal(cols[4], $"IncomeFrom (line {i + 1})");
            dto.IncomeTo = ParseDecimal(cols[5], $"IncomeTo (line {i + 1})");
            if (dto.IncomeFrom < 0 || dto.IncomeTo < dto.IncomeFrom)
                return BadRequest($"Invalid income range at line {i + 1}.");

            dto.Rate = ParseDecimal(cols[6], $"Rate (line {i + 1})");
            dto.Rate = NormalizeRate(dto.Rate);
            if (dto.Rate < 0)
                return BadRequest($"Rate darf nicht negativ sein (line {i + 1}).");

            dto.Remark = cols.Count >= 8 ? Clean(cols[7]) : null;

            parsed.Add(dto);
        }

        if (parsed.Count == 0)
            return BadRequest("No valid rows.");

        var groups = parsed
            .GroupBy(x => new { x.Canton, x.Code, x.PermitType, x.ChurchMember })
            .ToList();

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        foreach (var g in groups)
        {
            var toDelete = await _db.QstTariffs
                .Where(t =>
                    t.Canton == g.Key.Canton &&
                    t.Code == g.Key.Code &&
                    t.PermitType == g.Key.PermitType &&
                    t.ChurchMember == g.Key.ChurchMember)
                .ToListAsync(ct);

            if (toDelete.Count > 0)
                _db.QstTariffs.RemoveRange(toDelete);

            foreach (var d in g.OrderBy(x => x.IncomeFrom))
            {
                _db.QstTariffs.Add(new QstTariff
                {
                    Canton = d.Canton!,
                    Code = d.Code!,
                    PermitType = d.PermitType!,
                    ChurchMember = d.ChurchMember,
                    IncomeFrom = d.IncomeFrom,
                    IncomeTo = d.IncomeTo,
                    Rate = d.Rate,
                    Remark = d.Remark
                });
            }
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new
        {
            imported = parsed.Count,
            groups = groups.Count
        });
    }

    [HttpDelete("qst-tariffs/{id:int}")]
    public async Task<IActionResult> DeleteQstTariff(int id, CancellationToken ct = default)
    {
        var entity = await _db.QstTariffs.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity is null)
            return NotFound();

        _db.QstTariffs.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // --- helpers ---
    private static string? Clean(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static bool IsValidKey(string key) =>
        AllowedPrefixes.Any(p => key.StartsWith(p, StringComparison.Ordinal));

    private static bool IsValidValue(string key, decimal value)
    {
        if (key.StartsWith("Rounding.", StringComparison.Ordinal))
            return value == 0.01m || value == 0.05m;

        if (key is "ALV.CapAnnual"
            or "BVG.EntryThresholdAnnual"
            or "BVG.CoordinationDedAnnual"
            or "BVG.UpperLimitAnnual")
            return value >= 0;

        return value >= 0;
    }

    private static decimal NormalizeRate(decimal r)
    {
        if (r < 0m) return r;
        if (r > 1m) r = r / 100m;
        return r;
    }

    private static bool ParseBool(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        var v = s.Trim().ToLowerInvariant();

        return v switch
        {
            "1" => true,
            "true" => true,
            "yes" => true,
            "y" => true,
            "ja" => true,
            "t" => true,
            _ => false
        };
    }

    private static decimal ParseDecimal(string? s, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new InvalidOperationException($"{fieldName} is empty.");

        var raw = s.Trim();

        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var a))
            return a;

        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.GetCultureInfo("de-CH"), out var b))
            return b;

        raw = raw.Replace(',', '.');
        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var c))
            return c;

        throw new InvalidOperationException($"{fieldName} invalid decimal: '{s}'.");
    }

    private static List<string?> SplitCsvLine(string line, char delim)
    {
        var res = new List<string?>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && ch == delim)
            {
                res.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(ch);
        }

        res.Add(sb.ToString());
        return res;
    }
}
