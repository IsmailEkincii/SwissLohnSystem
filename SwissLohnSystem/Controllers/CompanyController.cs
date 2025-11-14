using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Companies;
using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;

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
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => c.ToDto())
                .ToListAsync();

            return ApiResponse<IEnumerable<CompanyDto>>.Ok(companies, "Firmenliste wurde erfolgreich geladen.");
        }

        // GET: api/Company/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> GetCompany(int id)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

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

            var entity = dto.ToEntity();
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

            entity.Apply(dto);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der Firma (Id={CompanyId}).", id);
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
            var exists = await _context.Companies.AsNoTracking().AnyAsync(c => c.Id == id);
            if (!exists)
                return NotFound(ApiResponse<IEnumerable<EmployeeDto>>.Fail("Firma wurde nicht gefunden."));

            var list = await _context.Employees
                .AsNoTracking()
                .Where(e => e.CompanyId == id)
                .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
                .Select(e => e.ToDto())   // 🔴 BURASI DÜZENLENDİ
                .ToListAsync();

            return ApiResponse<IEnumerable<EmployeeDto>>.Ok(list, "Mitarbeiterliste wurde erfolgreich geladen.");
        }
    }
}
