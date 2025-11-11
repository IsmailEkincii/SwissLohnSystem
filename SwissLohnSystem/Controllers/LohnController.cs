using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LohnController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LohnController(ApplicationDbContext context) => _context = context;

        // GET: api/Lohn
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<LohnDto>>>> GetLohns()
        {
            var lohns = await _context.Lohns
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => l.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<LohnDto>>.Ok(lohns, "Alle Lohnabrechnungen wurden erfolgreich geladen.");
        }

        // GET: api/Lohn/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> GetLohn(int id)
        {
            var lohn = await _context.Lohns
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lohn is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Lohnabrechnung wurde nicht gefunden."));

            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung erfolgreich geladen.");
        }

        // POST: api/Lohn/Calculate
        [HttpPost("Calculate")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> Calculate([FromBody] LohnCalculateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<LohnDto>.Fail("Ungültige Eingabedaten."));

            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee is null)
                return NotFound(ApiResponse<LohnDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            var date = new DateTime(dto.Year, dto.Month, 1);
            if (employee.StartDate > date || (employee.EndDate.HasValue && employee.EndDate.Value < date))
                return BadRequest(ApiResponse<LohnDto>.Fail("Mitarbeiter ist in diesem Monat nicht aktiv."));

            // Einstellungen
            var settings = await _context.Settings.AsNoTracking().ToListAsync();
            decimal ahv = settings.FirstOrDefault(s => s.Name == "AHV")?.Value ?? 0;
            decimal alv = settings.FirstOrDefault(s => s.Name == "ALV")?.Value ?? 0;
            decimal bvg = settings.FirstOrDefault(s => s.Name == "BVG")?.Value ?? 0;
            decimal steuer = settings.FirstOrDefault(s => s.Name == "Steuer")?.Value ?? 0;
            decimal childAllowanceValue = settings.FirstOrDefault(s => s.Name == "ChildAllowance")?.Value ?? 0;
            decimal holidayRate = settings.FirstOrDefault(s => s.Name == "HolidayRate")?.Value ?? 8.33m;
            decimal overtimeRate = settings.FirstOrDefault(s => s.Name == "OvertimeRate")?.Value ?? 1.25m;

            var workDays = await _context.WorkDays
                .Where(w => w.EmployeeId == dto.EmployeeId && w.Date.Month == dto.Month && w.Date.Year == dto.Year)
                .ToListAsync();

            decimal totalHours = employee.SalaryType == "Monthly"
                ? employee.MonthlyHours
                : workDays.Sum(w => w.HoursWorked);

            decimal totalOvertime = workDays.Sum(w => w.OvertimeHours);

            if (employee.SalaryType == "Hourly" && workDays.Count == 0)
                return BadRequest(ApiResponse<LohnDto>.Fail("Keine Arbeitszeit-Daten für diesen stundenbasierten Mitarbeiter gefunden."));

            decimal brutto = employee.SalaryType == "Monthly"
                ? employee.BruttoSalary
                : employee.HourlyRate * totalHours;

            decimal overtimePay = totalOvertime * employee.HourlyRate * overtimeRate;
            decimal holidayAllowance = brutto * holidayRate / 100;
            decimal totalDeductions = (brutto + overtimePay) * (ahv + alv + bvg + steuer) / 100;
            decimal childAllowance = employee.ChildCount * childAllowanceValue;
            decimal netSalary = brutto + overtimePay + holidayAllowance + childAllowance - totalDeductions;

            var lohn = new Lohn
            {
                EmployeeId = employee.Id,
                Month = dto.Month,
                Year = dto.Year,
                BruttoSalary = brutto,
                OvertimePay = overtimePay,
                HolidayAllowance = holidayAllowance,
                TotalDeductions = totalDeductions,
                ChildAllowance = childAllowance,
                NetSalary = netSalary,
                CreatedAt = DateTime.Now
            };

            _context.Lohns.Add(lohn);
            await _context.SaveChangesAsync();

            return ApiResponse<LohnDto>.Ok(lohn.ToDto(), "Lohnabrechnung erfolgreich berechnet und gespeichert.");
        }
    }
}
