using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.WorkDay;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkDayController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public WorkDayController(ApplicationDbContext context) => _context = context;

        // GET: api/WorkDay/Employee/5
        [HttpGet("Employee/{employeeId:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<WorkDayDto>>>> GetEmployeeWorkDays(int employeeId)
        {
            // Mitarbeiter existiert mi?
            var exists = await _context.Employees.AnyAsync(e => e.Id == employeeId);
            if (!exists)
                return NotFound(ApiResponse<IEnumerable<WorkDayDto>>.Fail("Mitarbeiter wurde nicht gefunden."));

            var workDays = await _context.WorkDays
                .AsNoTracking()
                .Where(w => w.EmployeeId == employeeId)
                .OrderBy(w => w.Date)
                .Select(w => w.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<WorkDayDto>>.Ok(workDays, "Arbeitszeiten erfolgreich geladen.");
        }

        // POST: api/WorkDay
        [HttpPost]
        public async Task<ActionResult<ApiResponse<WorkDayDto>>> PostWorkDay([FromBody] WorkDayCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<WorkDayDto>.Fail("Ungültige Eingabedaten."));

            // Mitarbeiter kontrol
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId);
            if (!employeeExists)
                return BadRequest(ApiResponse<WorkDayDto>.Fail("Ungültige Mitarbeiter-ID (EmployeeId)."));

            // Aynı gün için duplicate önleme (isteğe bağlı)
            var duplicate = await _context.WorkDays.AnyAsync(w => w.EmployeeId == dto.EmployeeId && w.Date.Date == dto.Date.Date);
            if (duplicate)
                return BadRequest(ApiResponse<WorkDayDto>.Fail("Für diesen Tag existiert bereits ein Eintrag."));

            var entity = dto.ToEntity();
            _context.WorkDays.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id },
                ApiResponse<WorkDayDto>.Ok(entity.ToDto(), "Arbeitszeit erfolgreich hinzugefügt."));
        }

        // GET: api/WorkDay/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<WorkDayDto>>> GetById(int id)
        {
            var w = await _context.WorkDays.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (w is null)
                return NotFound(ApiResponse<WorkDayDto>.Fail("Arbeitszeit nicht gefunden."));

            return ApiResponse<WorkDayDto>.Ok(w.ToDto(), "Arbeitszeit erfolgreich geladen.");
        }

        // PUT: api/WorkDay/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> PutWorkDay(int id, [FromBody] WorkDayUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Arbeitszeit-ID."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Eingabedaten."));

            var entity = await _context.WorkDays.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Arbeitszeit nicht gefunden."));

            // Mitarbeiter kontrol (EmployeeId değişmiş olabilir)
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId);
            if (!employeeExists)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Mitarbeiter-ID (EmployeeId)."));

            // Duplicate kontrol (aynı güne ikinci kayıt)
            var duplicate = await _context.WorkDays.AnyAsync(w =>
                w.EmployeeId == dto.EmployeeId &&
                w.Date.Date == dto.Date.Date &&
                w.Id != id);
            if (duplicate)
                return BadRequest(ApiResponse<string>.Fail("Für diesen Tag existiert bereits ein Eintrag."));

            entity.Apply(dto);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Arbeitszeit erfolgreich aktualisiert.");
        }

        // DELETE: api/WorkDay/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteWorkDay(int id)
        {
            var entity = await _context.WorkDays.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Arbeitszeit nicht gefunden."));

            _context.WorkDays.Remove(entity);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Arbeitszeit erfolgreich gelöscht.");
        }
    }
}
