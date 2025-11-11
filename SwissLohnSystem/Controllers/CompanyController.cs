using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Companies;   // CompanyDto, CompanyCreateDto, CompanyUpdateDto
using SwissLohnSystem.API.DTOs.Employees;  // EmployeeDto
using SwissLohnSystem.API.Mapping;
using SwissLohnSystem.API.Mappings;        // CompanyMapping (ToDto/ToEntity/Apply)
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;       // ApiResponse<T>
using System.Collections.Generic;
using System.Linq;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ApplicationDbContext context, ILogger<CompanyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Company
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDto>>>> GetCompanies()
        {
            var companies = await _context.Companies
                .OrderBy(c => c.Name)
                .Select(c => c.ToDto()) // ✅ Mapping extension
                .ToListAsync();

            return ApiResponse<IEnumerable<CompanyDto>>.Ok(companies, "Firmenliste wurde erfolgreich geladen.");
        }

        // GET: api/Company/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company is null)
                return NotFound(ApiResponse<CompanyDto>.Fail("Firma wurde nicht gefunden."));

            return ApiResponse<CompanyDto>.Ok(company.ToDto(), "Firma erfolgreich gefunden.");
        }

        // POST: api/Company
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> PostCompany([FromBody] CompanyCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CompanyDto>.Fail("Ungültige Daten wurden gesendet."));

            var entity = dto.ToEntity(); // ✅ Mapping extension
            _context.Companies.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = entity.Id },
                ApiResponse<CompanyDto>.Ok(entity.ToDto(), "Firma wurde erfolgreich hinzugefügt."));
        }

        // PUT: api/Company/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> PutCompany(int id, [FromBody] CompanyUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("ID stimmt nicht überein."));
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Eingabe."));

            var entity = await _context.Companies.FindAsync(id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Firma wurde nicht gefunden."));

            entity.Apply(dto); // ✅ Mapping extension

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der Firma.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Interner Fehler beim Aktualisieren."));
            }

            return ApiResponse<string>.Ok("Firmendaten wurden erfolgreich aktualisiert.");
        }

        // DELETE: api/Company/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company is null)
                return NotFound(ApiResponse<string>.Fail("Firma wurde nicht gefunden."));

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Firma wurde erfolgreich gelöscht.");
        }

        // GET: api/Company/5/Employees
        [HttpGet("{id:int}/Employees")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetCompanyEmployees(int id)
        {
            var exists = await _context.Companies.AnyAsync(c => c.Id == id);
            if (!exists)
                return NotFound(ApiResponse<IEnumerable<EmployeeDto>>.Fail("Firma wurde nicht gefunden."));

            var list = await _context.Employees
                .Where(e => e.CompanyId == id)
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => new EmployeeDto(
                    e.Id, e.CompanyId,
                    e.FirstName, e.LastName,
                    e.Email, e.Position,
                    e.BirthDate, e.MaritalStatus, e.ChildCount,
                    e.SalaryType, e.HourlyRate, e.MonthlyHours, e.BruttoSalary,
                    e.StartDate, e.EndDate, e.Active,
                    e.AHVNumber, e.Krankenkasse, e.BVGPlan,
                    e.PensumPercent, e.HolidayRate, e.OvertimeRate, e.WithholdingTaxCode,
                    e.Address, e.Zip, e.City, e.Phone
                ))
                .ToListAsync();

            return ApiResponse<IEnumerable<EmployeeDto>>.Ok(list, "Mitarbeiterliste wurde erfolgreich geladen.");
        }
    }
}
