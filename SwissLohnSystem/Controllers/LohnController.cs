using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LohnController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LohnController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Lohn
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<Lohn>>>> GetLohns()
        {
            var lohns = await _context.Lohns.Include(l => l.Employee).ToListAsync();
            return Ok(new ApiResponse<IEnumerable<Lohn>>
            {
                Success = true,
                Message = "Alle Lohnabrechnungen wurden erfolgreich geladen.",
                Data = lohns
            });
        }

        // GET: api/Lohn/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Lohn>>> GetLohn(int id)
        {
            var lohn = await _context.Lohns.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
            if (lohn == null)
            {
                return NotFound(new ApiResponse<Lohn>
                {
                    Success = false,
                    Message = "Lohnabrechnung wurde nicht gefunden."
                });
            }

            return Ok(new ApiResponse<Lohn>
            {
                Success = true,
                Message = "Lohnabrechnung erfolgreich geladen.",
                Data = lohn
            });
        }

        // POST: api/Lohn/Calculate
        [HttpPost("Calculate")]
        public async Task<ActionResult<ApiResponse<Lohn>>> CalculateLohn(int employeeId, int month, int year)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound(new ApiResponse<Lohn>
                {
                    Success = false,
                    Message = "Mitarbeiter wurde nicht gefunden."
                });
            }

            var date = new DateTime(year, month, 1);
            if (employee.StartDate > date || (employee.EndDate.HasValue && employee.EndDate.Value < date))
            {
                return BadRequest(new ApiResponse<Lohn>
                {
                    Success = false,
                    Message = "Mitarbeiter ist in diesem Monat nicht aktiv."
                });
            }

            // Einstellungen al
            var settings = await _context.Settings.ToListAsync();
            decimal ahv = settings.FirstOrDefault(s => s.Name == "AHV")?.Value ?? 0;
            decimal alv = settings.FirstOrDefault(s => s.Name == "ALV")?.Value ?? 0;
            decimal bvg = settings.FirstOrDefault(s => s.Name == "BVG")?.Value ?? 0;
            decimal steuer = settings.FirstOrDefault(s => s.Name == "Steuer")?.Value ?? 0;
            decimal childAllowanceValue = settings.FirstOrDefault(s => s.Name == "ChildAllowance")?.Value ?? 0;
            decimal holidayRate = settings.FirstOrDefault(s => s.Name == "HolidayRate")?.Value ?? 8.33m;
            decimal overtimeRate = settings.FirstOrDefault(s => s.Name == "OvertimeRate")?.Value ?? 1.25m;

            var workDays = await _context.WorkDays
                .Where(w => w.EmployeeId == employeeId && w.Date.Month == month && w.Date.Year == year)
                .ToListAsync();

            decimal totalHours = employee.SalaryType == "Monthly" ? employee.MonthlyHours : workDays.Sum(w => w.HoursWorked);
            decimal totalOvertime = workDays.Sum(w => w.OvertimeHours);

            if (employee.SalaryType == "Hourly" && workDays.Count == 0)
            {
                return BadRequest(new ApiResponse<Lohn>
                {
                    Success = false,
                    Message = "Keine Arbeitszeit-Daten für diesen stundenbasierten Mitarbeiter gefunden."
                });
            }

            decimal brutto = employee.SalaryType == "Monthly" ? employee.BruttoSalary : employee.HourlyRate * totalHours;
            decimal overtimePay = totalOvertime * employee.HourlyRate * overtimeRate;
            decimal holidayAllowance = brutto * holidayRate / 100;
            decimal totalDeductions = (brutto + overtimePay) * (ahv + alv + bvg + steuer) / 100;
            decimal childAllowance = employee.ChildCount * childAllowanceValue;
            decimal netSalary = brutto + overtimePay + holidayAllowance + childAllowance - totalDeductions;

            var lohn = new Lohn
            {
                EmployeeId = employee.Id,
                Month = month,
                Year = year,
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

            return Ok(new ApiResponse<Lohn>
            {
                Success = true,
                Message = "Lohnabrechnung erfolgreich berechnet und gespeichert.",
                Data = lohn
            });
        }
    }
}
