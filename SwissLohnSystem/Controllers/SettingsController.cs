using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Setting;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public SettingController(ApplicationDbContext context) => _context = context;

        // GET: api/Setting
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<SettingDto>>>> GetSettings()
        {
            var list = await _context.Settings
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .Select(s => s.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<SettingDto>>.Ok(list, "Einstellungen erfolgreich geladen.");
        }

        // GET: api/Setting/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<SettingDto>>> GetSetting(int id)
        {
            var s = await _context.Settings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (s is null)
                return NotFound(ApiResponse<SettingDto>.Fail("Einstellung wurde nicht gefunden."));

            return ApiResponse<SettingDto>.Ok(s.ToDto(), "Einstellung erfolgreich gefunden.");
        }

        // POST: api/Setting
        [HttpPost]
        public async Task<ActionResult<ApiResponse<SettingDto>>> PostSetting([FromBody] SettingCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<SettingDto>.Fail("Ungültige Eingabedaten."));

            // Opsiyonel: aynı isim var mı? (AHV/ALV vs.)
            var exists = await _context.Settings.AnyAsync(x => x.Name == dto.Name);
            if (exists)
                return BadRequest(ApiResponse<SettingDto>.Fail("Einstellungsname ist bereits vorhanden."));

            var entity = dto.ToEntity();
            _context.Settings.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSetting), new { id = entity.Id },
                ApiResponse<SettingDto>.Ok(entity.ToDto(), "Einstellung wurde erfolgreich erstellt."));
        }

        // PUT: api/Setting/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> PutSetting(int id, [FromBody] SettingUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("ID stimmt nicht überein."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Eingabe."));

            var entity = await _context.Settings.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Einstellung wurde nicht gefunden."));

            // Opsiyonel: isim çakışması
            var nameTaken = await _context.Settings.AnyAsync(x => x.Id != id && x.Name == dto.Name);
            if (nameTaken)
                return BadRequest(ApiResponse<string>.Fail("Einstellungsname ist bereits vorhanden."));

            entity.Apply(dto);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Einstellung wurde erfolgreich aktualisiert.");
        }

        // DELETE: api/Setting/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteSetting(int id)
        {
            var entity = await _context.Settings.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Einstellung wurde nicht gefunden."));

            _context.Settings.Remove(entity);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Einstellung wurde erfolgreich gelöscht.");
        }
    }
}
