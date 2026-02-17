using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Documents;
using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.DTOs.Payroll;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Responses;
using SwissLohnSystem.API.Services.Lohn;

namespace SwissLohnSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LohnController : ControllerBase
    {
        private readonly ILohnService _lohnService;
        private readonly ApplicationDbContext _db;

        public LohnController(ILohnService lohnService, ApplicationDbContext db)
        {
            _lohnService = lohnService;
            _db = db;
        }

        // =====================================================
        // POST: api/lohn/calculate
        // =====================================================
        [HttpPost("calculate")]
        public async Task<ActionResult<ApiResponse<LohnDto>>> Calculate(
            [FromBody] PayrollRequestDto request,
            CancellationToken ct)
        {
            try
            {
                var dto = await _lohnService.CalculateAsync(request, ct);

                return Ok(new ApiResponse<LohnDto>
                {
                    Success = true,
                    Data = dto,
                    Message = null
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<LohnDto>
                {
                    Success = false,
                    Data = default,
                    Message = ex.Message
                });
            }
        }

        // =====================================================
        // POST: api/lohn/{id}/finalize
        // =====================================================
        [HttpPost("{id:int}/finalize")]
        public async Task<ActionResult<ApiResponse<string>>> Finalize(int id, CancellationToken ct)
        {
            try
            {
                await _lohnService.FinalizeAsync(id, ct);
                return Ok(new ApiResponse<string> { Success = true, Data = "OK", Message = "Lohn finalized." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Data = null, Message = ex.Message });
            }
        }

        // =====================================================
        // GET: api/lohn/{id}
        // Details JSON for UI
        // =====================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<LohnDetailsDto>>> GetDetails(int id, CancellationToken ct)
        {
            try
            {
                var dto = await _lohnService.GetDetailsAsync(id, ct);
                return Ok(new ApiResponse<LohnDetailsDto> { Success = true, Data = dto, Message = null });
            }
            catch (Exception ex)
            {
                return NotFound(new ApiResponse<LohnDetailsDto> { Success = false, Data = null, Message = ex.Message });
            }
        }

        // =====================================================
        // GET: api/lohn/by-company/{companyId}?year=2026&month=1
        // Firma -> Löhne (monthly)
        // =====================================================
        [HttpGet("by-company/{companyId:int}")]
        public async Task<ActionResult<ApiResponse<List<CompanyMonthlyLohnDto>>>> GetByCompany(
    int companyId,
    [FromQuery] int year,
    [FromQuery] int month,
    CancellationToken ct)
        {
            try
            {
                var rows = await _lohnService.GetCompanyMonthlyAsync(companyId, year, month, ct);
                return Ok(new ApiResponse<List<CompanyMonthlyLohnDto>> { Success = true, Data = rows, Message = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<CompanyMonthlyLohnDto>> { Success = false, Data = null, Message = ex.Message });
            }
        }
        // GET: api/lohn/lohnausweis/{employeeId}?year=2024
        [HttpGet("lohnausweis/{employeeId:int}")]
        public async Task<ActionResult<ApiResponse<LohnausweisDto>>> GetLohnausweis(
            int employeeId,
            [FromQuery] int year,
            CancellationToken ct)
        {
            try
            {
                var dto = await _lohnService.GetLohnausweisAsync(employeeId, year, ct);
                return Ok(new ApiResponse<LohnausweisDto> { Success = true, Data = dto, Message = null });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<LohnausweisDto> { Success = false, Data = null, Message = ex.Message });
            }
        }
        // GET: api/lohn/lohnausweis/{employeeId}/pdf?year=2024
        [HttpGet("lohnausweis/{employeeId:int}/pdf")]
        public async Task<IActionResult> DownloadLohnausweisPdf(
            int employeeId,
            [FromQuery] int year,
            CancellationToken ct)
        {
            LohnausweisDto dto;
            try
            {
                dto = await _lohnService.GetLohnausweisAsync(employeeId, year, ct);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Data = null, Message = ex.Message });
            }

            // ✅ Kesin kural: incomplete ise PDF verme
            if (!dto.IsComplete)
            {
                var msg = $"Lohnausweis cannot be generated. MissingMonths=[{string.Join(",", dto.MissingMonths)}], NonFinalMonths=[{string.Join(",", dto.NonFinalMonths)}]";
                return Conflict(new ApiResponse<string> { Success = false, Data = null, Message = msg });
            }

            var document = new SwissLohnSystem.API.Documents.LohnausweisPdfDocument(dto);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Lohnausweis_{dto.Year}_Emp{dto.EmployeeId}.pdf");
        }

        // =====================================================
        // GET: api/lohn/{id}/pdf
        // =====================================================
        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DownloadPdf(int id, CancellationToken ct)
        {
            var lohn = await _db.Lohns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (lohn is null)
                return NotFound(new ApiResponse<string> { Success = false, Data = null, Message = "Lohn not found." });

            if (!lohn.IsFinal)
                return Conflict(new ApiResponse<string> { Success = false, Data = null, Message = "Lohn must be finalized before PDF export." });

            // ✅ FIX: tam details + items
            var details = await _lohnService.GetDetailsAsync(id, ct);

            var document = new LohnSlipPdfDocument(details);
            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Lohn_{lohn.Year:D4}_{lohn.Month:D2}_Emp{lohn.EmployeeId}.pdf");
        }

    }
}
