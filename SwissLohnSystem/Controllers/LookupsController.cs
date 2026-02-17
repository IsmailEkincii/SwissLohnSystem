using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.Responses;

namespace SwissLohnSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class LookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public LookupsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // GET: api/lookups/qst-codes?companyId=1&canton=ZH&permitType=B
        // =====================================================
        [HttpGet("qst-codes")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetQstCodes(
            [FromQuery] int companyId,
            [FromQuery] string canton,
            [FromQuery] string permitType,
            CancellationToken ct)
        {
            if (companyId <= 0)
                return BadRequest(ApiResponse<List<string>>.Fail("companyId is required."));

            if (string.IsNullOrWhiteSpace(canton))
                return BadRequest(ApiResponse<List<string>>.Fail("canton is required."));

            if (string.IsNullOrWhiteSpace(permitType))
                return BadRequest(ApiResponse<List<string>>.Fail("permitType is required."));

            canton = canton.Trim().ToUpperInvariant();
            permitType = permitType.Trim().ToUpperInvariant();

            var codes = await _db.QstTariffs
                .AsNoTracking()
                .Where(t =>
                    t.CompanyId == companyId &&
                    t.Canton == canton &&
                    t.PermitType == permitType &&
                    t.Code != null && t.Code != "")
                .Select(t => t.Code)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(ct);

            return ApiResponse<List<string>>.Ok(codes);
        }

    }
}
