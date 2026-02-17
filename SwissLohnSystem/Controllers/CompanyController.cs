using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Data.Seed;
using SwissLohnSystem.API.DTOs.Companies;
using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SwissLohnSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(
            ApplicationDbContext context,
            ILogger<CompanyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================
        // GET: api/company
        // =====================================================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDto>>>> GetCompanies(CancellationToken ct)
        {
            var companies = await _context.Companies
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => c.ToDto())
                .ToListAsync(ct);

            return ApiResponse<IEnumerable<CompanyDto>>
                .Ok(companies, "Firmenliste wurde erfolgreich geladen.");
        }

        // =====================================================
        // GET: api/company/{id}
        // =====================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> GetCompany(int id, CancellationToken ct)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (company is null)
                return NotFound(ApiResponse<CompanyDto>.Fail("Firma wurde nicht gefunden."));

            return ApiResponse<CompanyDto>
                .Ok(company.ToDto(), "Firma erfolgreich gefunden.");
        }

        // =====================================================
        // POST: api/company
        // CREATE + AUTO SETTINGS SEED 🔥
        // =====================================================
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> CreateCompany(
            [FromBody] CompanyCreateDto dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<CompanyDto>.Fail("Ungültige Daten wurden gesendet."));

            var entity = dto.ToEntity();

            _context.Companies.Add(entity);
            await _context.SaveChangesAsync(ct);

            // 🔥 Company-scoped default settings (Excel uyumlu)
            await CompanySettingsSeeder.EnsureCompanyDefaultsAsync(_context, entity.Id, ct);

            // EnsureCompanyDefaultsAsync içeride SaveChanges yapıyor ama "garanti" için bırakıyoruz
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(
                nameof(GetCompany),
                new { id = entity.Id },
                ApiResponse<CompanyDto>.Ok(entity.ToDto(), "Firma wurde erfolgreich hinzugefügt.")
            );
        }

        // =====================================================
        // PUT: api/company/{id}
        // =====================================================
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateCompany(
            int id,
            [FromBody] CompanyUpdateDto dto,
            CancellationToken ct)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("ID stimmt nicht überein."));

            var entity = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Firma wurde nicht gefunden."));

            entity.Apply(dto);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der Firma (Id={CompanyId})", id);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.Fail("Interner Fehler beim Aktualisieren.")
                );
            }

            return ApiResponse<string>.Ok("Firmendaten wurden erfolgreich aktualisiert.");
        }

        // =====================================================
        // DELETE: api/company/{id}
        // (soft delete önerilir ama şimdilik hard)
        // =====================================================
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCompany(int id, CancellationToken ct)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (company is null)
                return NotFound(ApiResponse<string>.Fail("Firma wurde nicht gefunden."));

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<string>.Ok("Firma wurde erfolgreich gelöscht.");
        }

        // =====================================================
        // GET: api/company/{id}/employees
        // =====================================================
        [HttpGet("{id:int}/employees")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetCompanyEmployees(int id, CancellationToken ct)
        {
            var list = await _context.Employees
                .AsNoTracking()
                .Where(e => e.CompanyId == id)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => e.ToDto())
                .ToListAsync(ct);

            return ApiResponse<IEnumerable<EmployeeDto>>
                .Ok(list, "Mitarbeiterliste wurde erfolgreich geladen.");
        }
    }
}
