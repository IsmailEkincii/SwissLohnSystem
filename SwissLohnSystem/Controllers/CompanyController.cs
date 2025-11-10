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
        public async Task<ActionResult<ApiResponse<IEnumerable<Company>>>> GetCompanies()
        {
            var companies = await _context.Companies.ToListAsync();
            return Ok(new ApiResponse<IEnumerable<Company>>
            {
                Success = true,
                Message = "Firmenliste wurde erfolgreich geladen.",
                Data = companies
            });
        }

        // GET: api/Company/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Company>>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound(new ApiResponse<Company>
                {
                    Success = false,
                    Message = "Firma wurde nicht gefunden."
                });
            }

            return Ok(new ApiResponse<Company>
            {
                Success = true,
                Message = "Firma erfolgreich gefunden.",
                Data = company
            });
        }

        // POST: api/Company
        [HttpPost]
        public async Task<ActionResult<ApiResponse<Company>>> PostCompany(Company company)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<Company>
                {
                    Success = false,
                    Message = "Ungültige Daten wurden gesendet."
                });

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, new ApiResponse<Company>
            {
                Success = true,
                Message = "Firma wurde erfolgreich hinzugefügt.",
                Data = company
            });
        }

        // PUT: api/Company/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> PutCompany(int id, Company company)
        {
            if (id != company.Id)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "ID stimmt nicht überein." });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string> { Success = false, Message = "Ungültige Eingabe." });

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.Companies.Any(e => e.Id == id))
                    return NotFound(new ApiResponse<string> { Success = false, Message = "Firma wurde nicht gefunden." });

                _logger.LogError(ex, "Fehler beim Aktualisieren der Firma.");
                throw;
            }

            return Ok(new ApiResponse<string> { Success = true, Message = "Firmendaten wurden erfolgreich aktualisiert." });
        }

        // DELETE: api/Company/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound(new ApiResponse<string> { Success = false, Message = "Firma wurde nicht gefunden." });

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Firma wurde erfolgreich gelöscht." });
        }
    }

    // ✅ Einheitliches API-Antwortmodell
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
