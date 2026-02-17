using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Payroll;
using SwissLohnSystem.API.DTOs.Qst;
using SwissLohnSystem.API.DTOs.Setting;
using SwissLohnSystem.API.Mappings;
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;

namespace SwissLohnSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SettingsController(ApplicationDbContext db) => _db = db;

        // =========================
        // SETTINGS (company-scoped)
        // =========================
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SettingDto>>>> Get([FromQuery] int companyId)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<List<SettingDto>>.Fail("companyId required."));

            var list = await _db.Settings.AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.Name)
                .Select(x => x.ToDto())
                .ToListAsync();

            return Ok(ApiResponse<List<SettingDto>>.Ok(list));
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse<object>>> UpsertBulk([FromQuery] int companyId, [FromBody] List<SettingUpsertDto> items)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<object>.Fail("companyId required."));
            items ??= new();

            foreach (var it in items)
            {
                if (string.IsNullOrWhiteSpace(it.Name))
                    return BadRequest(ApiResponse<object>.Fail("Setting name required."));
            }

            var existing = await _db.Settings
                .Where(x => x.CompanyId == companyId)
                .ToDictionaryAsync(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);

            foreach (var dto in items)
            {
                var key = dto.Name.Trim();
                if (existing.TryGetValue(key, out var ent))
                {
                    ent.ApplyUpsert(dto);
                }
                else
                {
                    var n = new Setting { CompanyId = companyId, Name = key };
                    n.ApplyUpsert(dto);
                    _db.Settings.Add(n);
                }
            }

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { updated = items.Count }, "Settings saved."));
        }

        // =========================
        // QST TARIFFS (company-scoped)
        // =========================
        [HttpGet("qst-tariffs")]
        public async Task<ActionResult<ApiResponse<List<QstTariffDto>>>> GetQst([FromQuery] int companyId)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<List<QstTariffDto>>.Fail("companyId required."));

            var list = await _db.QstTariffs.AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .OrderBy(x => x.Canton).ThenBy(x => x.Code).ThenBy(x => x.PermitType).ThenBy(x => x.ChurchMember).ThenBy(x => x.IncomeFrom)
                .Select(x => new QstTariffDto
                {
                    Id = x.Id,
                    CompanyId = x.CompanyId,
                    Canton = x.Canton,
                    Code = x.Code,
                    PermitType = x.PermitType,
                    ChurchMember = x.ChurchMember,
                    IncomeFrom = x.IncomeFrom,
                    IncomeTo = x.IncomeTo,
                    Rate = x.Rate,
                    Remark = x.Remark
                })
                .ToListAsync();

            return Ok(ApiResponse<List<QstTariffDto>>.Ok(list));
        }

        [HttpPost("qst-tariffs")]
        public async Task<ActionResult<ApiResponse<QstTariffDto>>> CreateQst([FromQuery] int companyId, [FromBody] QstTariffDto dto)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<QstTariffDto>.Fail("companyId required."));

            dto.Canton = (dto.Canton ?? "ZH").Trim().ToUpperInvariant();
            dto.Code = dto.Code.Trim().ToUpperInvariant();
            dto.PermitType = (dto.PermitType ?? "B").Trim().ToUpperInvariant();

            if (dto.IncomeFrom < 0 || dto.IncomeTo < dto.IncomeFrom)
                return BadRequest(ApiResponse<QstTariffDto>.Fail("Invalid income range."));

            if (dto.Rate < 0)
                return BadRequest(ApiResponse<QstTariffDto>.Fail("Rate must be >= 0."));


            var ent = new QstTariff
            {
                CompanyId = companyId,
                Canton = dto.Canton,
                Code = dto.Code,
                PermitType = dto.PermitType,
                ChurchMember = dto.ChurchMember,
                IncomeFrom = dto.IncomeFrom,
                IncomeTo = dto.IncomeTo,
                Rate = dto.Rate,
                Remark = string.IsNullOrWhiteSpace(dto.Remark) ? null : dto.Remark.Trim()
            };

            _db.QstTariffs.Add(ent);
            await _db.SaveChangesAsync();

            dto.Id = ent.Id;
            dto.CompanyId = companyId;

            return Ok(ApiResponse<QstTariffDto>.Ok(dto, "QST created."));
        }

        [HttpPut("qst-tariffs")]
        public async Task<ActionResult<ApiResponse<object>>> SaveQst([FromQuery] int companyId, [FromBody] List<QstTariffDto> items)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<object>.Fail("companyId required."));
            items ??= new();

            var ids = items.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
            var existing = await _db.QstTariffs.Where(x => x.CompanyId == companyId && ids.Contains(x.Id)).ToListAsync();
            var map = existing.ToDictionary(x => x.Id);

            foreach (var dto in items)
            {
                if (dto.Id <= 0) continue;
                if (!map.TryGetValue(dto.Id, out var ent)) continue;

                ent.Canton = (dto.Canton ?? "ZH").Trim().ToUpperInvariant();
                ent.Code = dto.Code.Trim().ToUpperInvariant();
                ent.PermitType = (dto.PermitType ?? "B").Trim().ToUpperInvariant();
                ent.ChurchMember = dto.ChurchMember;
                ent.IncomeFrom = dto.IncomeFrom;
                ent.IncomeTo = dto.IncomeTo;
                ent.Rate = dto.Rate;
                ent.Remark = string.IsNullOrWhiteSpace(dto.Remark) ? null : dto.Remark.Trim();
            }

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { updated = existing.Count }, "QST saved."));
        }

        [HttpDelete("qst-tariffs/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteQst(int id, [FromQuery] int companyId)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<object>.Fail("companyId required."));

            var ent = await _db.QstTariffs.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
            if (ent is null) return NotFound(ApiResponse<object>.Fail("Not found."));

            _db.QstTariffs.Remove(ent);
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { deleted = id }, "Deleted."));
        }

        // =========================
        // BVG PLANS (company-scoped)
        // =========================
        [HttpGet("bvg-plans")]
        public async Task<ActionResult<ApiResponse<List<BvgPlanListItemDto>>>> GetBvgPlans([FromQuery] int companyId)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<List<BvgPlanListItemDto>>.Fail("companyId required."));

            var list = await _db.BvgPlans.AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .OrderByDescending(x => x.Year).ThenBy(x => x.PlanCode)
                .Select(x => new BvgPlanListItemDto { Code = x.PlanCode, Year = x.Year, Name = x.PlanBaseCode })
                .ToListAsync();

            return Ok(ApiResponse<List<BvgPlanListItemDto>>.Ok(list));
        }

        [HttpGet("bvg-plans/{planCode}")]
        public async Task<ActionResult<ApiResponse<BvgPlanDetailDto>>> GetBvgPlan([FromRoute] string planCode, [FromQuery] int companyId)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<BvgPlanDetailDto>.Fail("companyId required."));

            planCode = planCode.Trim().ToUpperInvariant();

            var p = await _db.BvgPlans.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.PlanCode == planCode);

            if (p is null) return NotFound(ApiResponse<BvgPlanDetailDto>.Fail("BVG plan not found."));

            var dto = new BvgPlanDetailDto
            {
                CompanyId = p.CompanyId,
                PlanCode = p.PlanCode,
                PlanBaseCode = p.PlanBaseCode,
                Year = p.Year,
                CoordinationDedAnnual = p.CoordinationDedAnnual,
                EntryThresholdAnnual = p.EntryThresholdAnnual,
                UpperLimitAnnual = p.UpperLimitAnnual,
                Rate25_34_Employee = p.Rate25_34_Employee,
                Rate25_34_Employer = p.Rate25_34_Employer,
                Rate35_44_Employee = p.Rate35_44_Employee,
                Rate35_44_Employer = p.Rate35_44_Employer,
                Rate45_54_Employee = p.Rate45_54_Employee,
                Rate45_54_Employer = p.Rate45_54_Employer,
                Rate55_65_Employee = p.Rate55_65_Employee,
                Rate55_65_Employer = p.Rate55_65_Employer
            };

            return Ok(ApiResponse<BvgPlanDetailDto>.Ok(dto));
        }

        [HttpPost("bvg-plans")]
        public async Task<ActionResult<ApiResponse<object>>> CreateOrUpdateBvg([FromQuery] int companyId, [FromBody] CreateOrUpdateBvgPlanDto dto)
        {
            if (companyId <= 0) return BadRequest(ApiResponse<object>.Fail("companyId required."));

            var baseCode = (dto.PlanBaseCode ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(baseCode)) return BadRequest(ApiResponse<object>.Fail("PlanBaseCode required."));
            if (dto.Year < 2000 || dto.Year > 2100) return BadRequest(ApiResponse<object>.Fail("Invalid year."));

            var planCode = $"{baseCode}_{dto.Year}".ToUpperInvariant();

            var ent = await _db.BvgPlans.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.PlanCode == planCode);
            if (ent is null)
            {
                ent = new BvgPlan { CompanyId = companyId, PlanCode = planCode, PlanBaseCode = baseCode, Year = dto.Year };
                _db.BvgPlans.Add(ent);
            }

            ent.PlanBaseCode = baseCode;
            ent.Year = dto.Year;
            ent.CoordinationDedAnnual = dto.CoordinationDedAnnual;
            ent.EntryThresholdAnnual = dto.EntryThresholdAnnual;
            ent.UpperLimitAnnual = dto.UpperLimitAnnual;

            ent.Rate25_34_Employee = dto.Rate25_34_Employee;
            ent.Rate25_34_Employer = dto.Rate25_34_Employer;
            ent.Rate35_44_Employee = dto.Rate35_44_Employee;
            ent.Rate35_44_Employer = dto.Rate35_44_Employer;
            ent.Rate45_54_Employee = dto.Rate45_54_Employee;
            ent.Rate45_54_Employer = dto.Rate45_54_Employer;
            ent.Rate55_65_Employee = dto.Rate55_65_Employee;
            ent.Rate55_65_Employer = dto.Rate55_65_Employer;

            ent.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { planCode }, "BVG saved."));
        }
    }
}
