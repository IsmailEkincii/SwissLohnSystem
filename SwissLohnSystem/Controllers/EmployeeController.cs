using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Mappings;   // EmployeeMappings
using SwissLohnSystem.API.Responses;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ApplicationDbContext context, ILogger<EmployeeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetEmployees()
        {
            var list = await _context.Employees
                .AsNoTracking()
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => e.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<EmployeeDto>>.Ok(list, "Mitarbeiterliste wurde erfolgreich geladen.");
        }

        // GET: api/Employee/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployee(int id)
        {
            var e = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e is null)
                return NotFound(ApiResponse<EmployeeDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            return ApiResponse<EmployeeDto>.Ok(e.ToDto(), "Mitarbeiter erfolgreich gefunden.");
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> PostEmployee([FromBody] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Ungültige Eingabedaten."));

            // Firma var mı?
            var companyExists = await _context.Companies.AsNoTracking().AnyAsync(c => c.Id == dto.CompanyId);
            if (!companyExists)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Ungültige Firmen-ID (CompanyId)."));

            // SalaryType kontrol
            if (dto.SalaryType != "Monthly" && dto.SalaryType != "Hourly")
                return BadRequest(ApiResponse<EmployeeDto>.Fail("SalaryType muss 'Monthly' oder 'Hourly' sein."));

            if (dto.SalaryType == "Monthly" && dto.BruttoSalary <= 0)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Bruttolohn ist erforderlich (> 0) für Monatslohn."));
            if (dto.SalaryType == "Hourly" && dto.HourlyRate <= 0)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Stundenlohn ist erforderlich (> 0) für Stundenlohn."));

            // Tarih kontrol
            if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Enddatum darf nicht vor dem Startdatum liegen."));

            // AHV gevşek format kontrolü (opsiyonel alan)
            if (!string.IsNullOrWhiteSpace(dto.AHVNumber))
            {
                var ok = Regex.IsMatch(dto.AHVNumber.Trim(), @"^(756\.\d{4}\.\d{4}\.\d{2}|756\d{10})$");
                if (!ok) return BadRequest(ApiResponse<EmployeeDto>.Fail("Ungültige AHV-Nummer. Beispiel: 756.1234.5678.97"));
            }

            try
            {
                var entity = dto.ToEntity(); // mapping
                _context.Employees.Add(entity);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEmployee), new { id = entity.Id },
                    ApiResponse<EmployeeDto>.Ok(entity.ToDto(), "Mitarbeiter wurde erfolgreich hinzugefügt."));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Fehler beim Speichern des Mitarbeiters.");
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Speichern fehlgeschlagen (FK/Constraint)."));
            }
        }

        // PUT: api/Employee/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> PutEmployee(int id, [FromBody] EmployeeUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("ID stimmt nicht überein."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Eingabe."));

            var entity = await _context.Employees.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Mitarbeiter wurde nicht gefunden."));

            // SalaryType kontrol
            if (dto.SalaryType != "Monthly" && dto.SalaryType != "Hourly")
                return BadRequest(ApiResponse<string>.Fail("SalaryType muss 'Monthly' oder 'Hourly' sein."));

            if (dto.SalaryType == "Monthly" && dto.BruttoSalary <= 0)
                return BadRequest(ApiResponse<string>.Fail("Bruttolohn ist erforderlich (> 0) für Monatslohn."));
            if (dto.SalaryType == "Hourly" && dto.HourlyRate <= 0)
                return BadRequest(ApiResponse<string>.Fail("Stundenlohn ist erforderlich (> 0) für Stundenlohn."));

            if (dto.EndDate.HasValue && dto.EndDate.Value < dto.StartDate)
                return BadRequest(ApiResponse<string>.Fail("Enddatum darf nicht vor dem Startdatum liegen."));

            // Firma var mı? (Company değişmiş olabilir)
            var companyExists = await _context.Companies.AsNoTracking().AnyAsync(c => c.Id == dto.CompanyId);
            if (!companyExists)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Firmen-ID (CompanyId)."));

            // AHV kontrol (opsiyonel)
            if (!string.IsNullOrWhiteSpace(dto.AHVNumber))
            {
                var ok = Regex.IsMatch(dto.AHVNumber.Trim(), @"^(756\.\d{4}\.\d{4}\.\d{2}|756\d{10})$");
                if (!ok) return BadRequest(ApiResponse<string>.Fail("Ungültige AHV-Nummer. Beispiel: 756.1234.5678.97"));
            }

            entity.Apply(dto); // mapping

            try
            {
                await _context.SaveChangesAsync();
                return ApiResponse<string>.Ok("Mitarbeiterdaten erfolgreich aktualisiert.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await _context.Employees.AsNoTracking().AnyAsync(e => e.Id == id))
                    return NotFound(ApiResponse<string>.Fail("Mitarbeiter wurde nicht gefunden."));

                _logger.LogError(ex, "Fehler beim Aktualisieren des Mitarbeiters (Id={EmployeeId}).", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Interner Fehler beim Aktualisieren."));
            }
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteEmployee(int id)
        {
            var entity = await _context.Employees.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Mitarbeiter wurde nicht gefunden."));

            _context.Employees.Remove(entity);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Mitarbeiter erfolgreich gelöscht.");
        }
    }
}
