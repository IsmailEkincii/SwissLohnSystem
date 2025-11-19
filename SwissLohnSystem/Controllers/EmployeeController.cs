using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwissLohnSystem.API.Data;
using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Models;
using SwissLohnSystem.API.Responses;

namespace SwissLohnSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // GET: api/Employee/{id}
        // UI: Employees/Edit + Employees/Details
        // =====================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetById(int id)
        {
            var e = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e is null)
                return NotFound(ApiResponse<EmployeeDto>.Fail("Mitarbeiter wurde nicht gefunden."));

            var dto = MapToDto(e);
            return ApiResponse<EmployeeDto>.Ok(dto, "Mitarbeiter erfolgreich geladen.");
        }

        // =====================================================
        // POST: api/Employee
        // UI: Companies/Employees/Create (EmployeeCreateDtoForApi)
        // =====================================================
        [HttpPost]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create([FromBody] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return BadRequest(ApiResponse<EmployeeDto>.Fail(
                    string.IsNullOrWhiteSpace(errors) ? "Ungültige Eingabedaten." : errors
                ));
            }

            // Firma var mı?
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId);
            if (!companyExists)
                return BadRequest(ApiResponse<EmployeeDto>.Fail("Ungültige Firmen-ID (CompanyId)."));

            var entity = new Employee();
            ApplyCreate(entity, dto);

            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = MapToDto(entity);

            return CreatedAtAction(nameof(GetById),
                new { id = entity.Id },
                ApiResponse<EmployeeDto>.Ok(resultDto, "Mitarbeiter wurde erfolgreich erstellt."));
        }

        // =====================================================
        // PUT: api/Employee/{id}
        // UI: Employees/Edit (EmployeeUpdateDtoForApi)
        // =====================================================
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> Update(int id, [FromBody] EmployeeUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Mitarbeiter-ID."));

            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ",
                    ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return BadRequest(ApiResponse<string>.Fail(
                    string.IsNullOrWhiteSpace(errors) ? "Ungültige Eingabedaten." : errors
                ));
            }

            var entity = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (entity is null)
                return NotFound(ApiResponse<string>.Fail("Mitarbeiter wurde nicht gefunden."));

            // Firma var mı?
            var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId);
            if (!companyExists)
                return BadRequest(ApiResponse<string>.Fail("Ungültige Firmen-ID (CompanyId)."));

            ApplyUpdate(entity, dto);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Mitarbeiter erfolgreich aktualisiert.");
        }

        // İstersen ileride DELETE de ekleyebiliriz:
        // DELETE: api/Employee/{id}
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

        // =====================================================
        // --------- MAPPING HELPERS (ENTITY <-> DTO) ----------
        // =====================================================

        private static EmployeeDto MapToDto(Employee e) => new EmployeeDto
        {
            Id = e.Id,
            CompanyId = e.CompanyId,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Position = e.Position,
            BirthDate = e.BirthDate,
            MaritalStatus = e.MaritalStatus,
            ChildCount = e.ChildCount,
            SalaryType = e.SalaryType,
            HourlyRate = e.HourlyRate,
            MonthlyHours = e.MonthlyHours,
            BruttoSalary = e.BruttoSalary,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            Active = e.Active,

            AHVNumber = e.AHVNumber,
            Krankenkasse = e.Krankenkasse,
            BVGPlan = e.BVGPlan,

            PensumPercent = e.PensumPercent,
            HolidayRate = e.HolidayRate,
            OvertimeRate = e.OvertimeRate,
            WithholdingTaxCode = e.WithholdingTaxCode,

            WeeklyHours = e.WeeklyHours,
            ApplyAHV = e.ApplyAHV,
            ApplyALV = e.ApplyALV,
            ApplyNBU = e.ApplyNBU,
            ApplyBU = e.ApplyBU,
            ApplyBVG = e.ApplyBVG,
            ApplyFAK = e.ApplyFAK,
            ApplyQST = e.ApplyQST,

            HolidayEligible = e.HolidayEligible,
            ThirteenthEligible = e.ThirteenthEligible,
            ThirteenthProrated = e.ThirteenthProrated,

            PermitType = e.PermitType,
            ChurchMember = e.ChurchMember,
            Canton = e.Canton,

            Address = e.Address,
            Zip = e.Zip,
            City = e.City,
            Phone = e.Phone
        };

        private static void ApplyCreate(Employee e, EmployeeCreateDto dto)
        {
            e.CompanyId = dto.CompanyId;
            e.FirstName = dto.FirstName.Trim();
            e.LastName = dto.LastName.Trim();
            e.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            e.Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim();

            e.BirthDate = dto.BirthDate;
            e.MaritalStatus = string.IsNullOrWhiteSpace(dto.MaritalStatus) ? null : dto.MaritalStatus.Trim();
            e.ChildCount = dto.ChildCount;

            e.SalaryType = dto.SalaryType; // "Monthly" | "Hourly"
            e.HourlyRate = dto.HourlyRate;
            e.MonthlyHours = dto.MonthlyHours;
            e.BruttoSalary = dto.BruttoSalary;

            e.StartDate = dto.StartDate;
            e.EndDate = dto.EndDate;
            e.Active = dto.Active;

            e.AHVNumber = string.IsNullOrWhiteSpace(dto.AHVNumber) ? null : dto.AHVNumber.Trim();
            e.Krankenkasse = string.IsNullOrWhiteSpace(dto.Krankenkasse) ? null : dto.Krankenkasse.Trim();
            e.BVGPlan = string.IsNullOrWhiteSpace(dto.BVGPlan) ? null : dto.BVGPlan.Trim();

            e.PensumPercent = dto.PensumPercent;
            e.HolidayRate = dto.HolidayRate;
            e.OvertimeRate = dto.OvertimeRate;
            e.WithholdingTaxCode = string.IsNullOrWhiteSpace(dto.WithholdingTaxCode)
                ? null
                : dto.WithholdingTaxCode.Trim();

            e.WeeklyHours = dto.WeeklyHours;
            e.ApplyAHV = dto.ApplyAHV;
            e.ApplyALV = dto.ApplyALV;
            e.ApplyNBU = dto.ApplyNBU;
            e.ApplyBU = dto.ApplyBU;
            e.ApplyBVG = dto.ApplyBVG;
            e.ApplyFAK = dto.ApplyFAK;
            e.ApplyQST = dto.ApplyQST;

            e.HolidayEligible = dto.HolidayEligible;
            e.ThirteenthEligible = dto.ThirteenthEligible;
            e.ThirteenthProrated = dto.ThirteenthProrated;

            e.PermitType = string.IsNullOrWhiteSpace(dto.PermitType) ? "B" : dto.PermitType.Trim();
            e.ChurchMember = dto.ChurchMember;
            e.Canton = string.IsNullOrWhiteSpace(dto.Canton) ? "ZH" : dto.Canton.Trim();

            e.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            e.Zip = string.IsNullOrWhiteSpace(dto.Zip) ? null : dto.Zip.Trim();
            e.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
            e.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        }

        private static void ApplyUpdate(Employee e, EmployeeUpdateDto dto)
        {
            e.CompanyId = dto.CompanyId;
            e.FirstName = dto.FirstName.Trim();
            e.LastName = dto.LastName.Trim();
            e.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            e.Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim();

            e.BirthDate = dto.BirthDate;
            e.MaritalStatus = string.IsNullOrWhiteSpace(dto.MaritalStatus) ? null : dto.MaritalStatus.Trim();
            e.ChildCount = dto.ChildCount;

            e.SalaryType = dto.SalaryType;
            e.HourlyRate = dto.HourlyRate;
            e.MonthlyHours = dto.MonthlyHours;
            e.BruttoSalary = dto.BruttoSalary;

            e.StartDate = dto.StartDate;
            e.EndDate = dto.EndDate;
            e.Active = dto.Active;

            e.AHVNumber = string.IsNullOrWhiteSpace(dto.AHVNumber) ? null : dto.AHVNumber.Trim();
            e.Krankenkasse = string.IsNullOrWhiteSpace(dto.Krankenkasse) ? null : dto.Krankenkasse.Trim();
            e.BVGPlan = string.IsNullOrWhiteSpace(dto.BVGPlan) ? null : dto.BVGPlan.Trim();

            e.PensumPercent = dto.PensumPercent;
            e.HolidayRate = dto.HolidayRate;
            e.OvertimeRate = dto.OvertimeRate;
            e.WithholdingTaxCode = string.IsNullOrWhiteSpace(dto.WithholdingTaxCode)
                ? null
                : dto.WithholdingTaxCode.Trim();

            e.WeeklyHours = dto.WeeklyHours;
            e.ApplyAHV = dto.ApplyAHV;
            e.ApplyALV = dto.ApplyALV;
            e.ApplyNBU = dto.ApplyNBU;
            e.ApplyBU = dto.ApplyBU;
            e.ApplyBVG = dto.ApplyBVG;
            e.ApplyFAK = dto.ApplyFAK;
            e.ApplyQST = dto.ApplyQST;

            e.HolidayEligible = dto.HolidayEligible;
            e.ThirteenthEligible = dto.ThirteenthEligible;
            e.ThirteenthProrated = dto.ThirteenthProrated;

            e.PermitType = string.IsNullOrWhiteSpace(dto.PermitType) ? "B" : dto.PermitType.Trim();
            e.ChurchMember = dto.ChurchMember;
            e.Canton = string.IsNullOrWhiteSpace(dto.Canton) ? "ZH" : dto.Canton.Trim();

            e.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            e.Zip = string.IsNullOrWhiteSpace(dto.Zip) ? null : dto.Zip.Trim();
            e.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
            e.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        }
    }
}
