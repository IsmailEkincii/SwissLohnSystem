using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Responses;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public EmployeeController(ApplicationDbContext context) => _context = context;

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(int id)
        {
            var e = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (e is null)
                return NotFound(ApiResponse<EmployeeDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            return ApiResponse<EmployeeDto>.Ok(e.ToDto(), "Mitarbeiter erfolgreich geladen.");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create([FromBody] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Ungültige Eingabedaten."));

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId);
            if (!companyExists)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Ungültige Firmen-ID (CompanyId)."));

            var entity = dto.ToEntity();
            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = entity.Id },
                ApiResponse<EmployeeDto>.Ok(entity.ToDto(), "Mitarbeiter wurde erfolgreich erstellt."));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(int id, [FromBody] EmployeeUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Mitarbeiter-ID."));

            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Eingabedaten."));

            var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Mitarbeiter wurde nicht gefunden."));

            var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId);
            if (!companyExists)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Firmen-ID (CompanyId)."));

            entity.Apply(dto);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Mitarbeiter erfolgreich aktualisiert.");
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Delete(int id)
        {
            var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Mitarbeiter wurde nicht gefunden."));

            _context.Employees.Remove(entity);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Mitarbeiter wurde gelöscht.");
        }

    }
}
