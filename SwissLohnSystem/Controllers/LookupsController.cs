using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Qst;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public LookupsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // =====================================================
        // GET /api/Lookups/qst-tariffs?canton=ZH&permit=B&church=true
        // UI veya diğer servisler için "lookup" tarzı hafif endpoint
        // =====================================================
        [HttpGet("qst-tariffs")]
        public async Task<ActionResult<IEnumerable<QstTariffDto>>> GetQstTariffs(
            [FromQuery] string? canton = null,
            [FromQuery] string? permit = null,
            [FromQuery] bool? church = null,
            CancellationToken ct = default)
        {
            var q = _db.QstTariffs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(canton))
            {
                var c = canton.Trim().ToUpperInvariant();
                q = q.Where(t => t.Canton == c);
            }

            if (!string.IsNullOrWhiteSpace(permit))
            {
                var p = permit.Trim().ToUpperInvariant();
                q = q.Where(t => t.PermitType == p);
            }

            if (church.HasValue)
            {
                q = q.Where(t => t.ChurchMember == church.Value);
            }

            var list = await q
                .OrderBy(t => t.Canton)
                .ThenBy(t => t.Code)
                .ThenBy(t => t.PermitType)
                .ThenBy(t => t.ChurchMember)
                .ThenBy(t => t.IncomeFrom)
                .Select(t => new QstTariffDto
                {
                    Id = t.Id,
                    Canton = t.Canton,
                    Code = t.Code,
                    PermitType = t.PermitType,
                    ChurchMember = t.ChurchMember,
                    IncomeFrom = t.IncomeFrom,
                    IncomeTo = t.IncomeTo,
                    Rate = t.Rate,
                    Remark = t.Remark
                })
                .ToListAsync(ct);

            return Ok(list);
        }
    }
}
