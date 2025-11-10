using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<ActionResult<ApiResponse<IEnumerable<Employee>>>> GetEmployees()
        {
            var employees = await _context.Employees.Include(e => e.Company).ToListAsync();
            return Ok(new ApiResponse<IEnumerable<Employee>>
            {
                Success = true,
                Message = "Mitarbeiterliste wurde erfolgreich geladen.",
                Data = employees
            });
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Employee>>> GetEmployee(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Company)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Mitarbeiter wurde nicht gefunden."
                });
            }

            return Ok(new ApiResponse<Employee>
            {
                Success = true,
                Message = "Mitarbeiter erfolgreich gefunden.",
                Data = employee
            });
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Employee>>> PostEmployee(Employee employee)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Ungültige Eingabedaten."
                });

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new ApiResponse<Employee>
            {
                Success = true,
                Message = "Mitarbeiter wurde erfolgreich hinzugefügt.",
                Data = employee
            });
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "ID stimmt nicht überein." });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Ungültige Eingabe." });

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Employees.Any(e => e.Id == id))
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Mitarbeiter wurde nicht gefunden." });

                _logger.LogError(ex, "Fehler beim Aktualisieren des Mitarbeiters.");
                throw;
            }

            return Ok(new ApiResponse<string> { Success = true, Message = "Mitarbeiterdaten erfolgreich aktualisiert." });
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Mitarbeiter wurde nicht gefunden." });

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Mitarbeiter erfolgreich gelöscht." });
        }
    }
}
