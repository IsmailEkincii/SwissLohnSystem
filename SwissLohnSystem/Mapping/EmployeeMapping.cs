using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class EmployeeMapping
    {
        public static EmployeeDto ToDto(this Employee e) => new EmployeeDto
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
            WeeklyHours = e.WeeklyHours,
            PensumPercent = e.PensumPercent,
            HolidayRate = e.HolidayRate,
            OvertimeRate = e.OvertimeRate,
            HolidayEligible = e.HolidayEligible,
            ThirteenthEligible = e.ThirteenthEligible,
            ThirteenthProrated = e.ThirteenthProrated,
            ApplyAHV = e.ApplyAHV,
            ApplyALV = e.ApplyALV,
            ApplyNBU = e.ApplyNBU,
            ApplyBU = e.ApplyBU,
            ApplyBVG = e.ApplyBVG,
            ApplyFAK = e.ApplyFAK,
            ApplyQST = e.ApplyQST,
            PermitType = e.PermitType,
            ChurchMember = e.ChurchMember,
            Canton = e.Canton,
            WithholdingTaxCode = e.WithholdingTaxCode,
            AHVNumber = e.AHVNumber,
            Krankenkasse = e.Krankenkasse,
            BVGPlan = e.BVGPlan,
            Address = e.Address,
            Zip = e.Zip,
            City = e.City,
            Phone = e.Phone
        };

        public static EmployeeListItemDto ToListItem(this Employee e) => new EmployeeListItemDto
        {
            Id = e.Id,
            CompanyId = e.CompanyId,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Position = e.Position,
            Active = e.Active
        };

        public static Employee ToEntity(this EmployeeCreateDto dto) => new Employee
        {
            CompanyId = dto.CompanyId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim(),
            BirthDate = dto.BirthDate,
            MaritalStatus = string.IsNullOrWhiteSpace(dto.MaritalStatus) ? null : dto.MaritalStatus.Trim(),
            ChildCount = dto.ChildCount,
            SalaryType = dto.SalaryType,
            HourlyRate = dto.HourlyRate,
            MonthlyHours = dto.MonthlyHours,
            BruttoSalary = dto.BruttoSalary,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Active = dto.Active,
            WeeklyHours = dto.WeeklyHours,
            PensumPercent = dto.PensumPercent,
            HolidayRate = dto.HolidayRate,
            OvertimeRate = dto.OvertimeRate,
            HolidayEligible = dto.HolidayEligible,
            ThirteenthEligible = dto.ThirteenthEligible,
            ThirteenthProrated = dto.ThirteenthProrated,
            ApplyAHV = dto.ApplyAHV,
            ApplyALV = dto.ApplyALV,
            ApplyNBU = dto.ApplyNBU,
            ApplyBU = dto.ApplyBU,
            ApplyBVG = dto.ApplyBVG,
            ApplyFAK = dto.ApplyFAK,
            ApplyQST = dto.ApplyQST,
            PermitType = dto.PermitType,
            ChurchMember = dto.ChurchMember,
            Canton = dto.Canton,
            WithholdingTaxCode = dto.WithholdingTaxCode,
            AHVNumber = dto.AHVNumber,
            Krankenkasse = dto.Krankenkasse,
            BVGPlan = dto.BVGPlan,
            Address = dto.Address,
            Zip = dto.Zip,
            City = dto.City,
            Phone = dto.Phone
        };

        public static void Apply(this Employee e, EmployeeUpdateDto dto)
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
            e.WeeklyHours = dto.WeeklyHours;
            e.PensumPercent = dto.PensumPercent;
            e.HolidayRate = dto.HolidayRate;
            e.OvertimeRate = dto.OvertimeRate;
            e.HolidayEligible = dto.HolidayEligible;
            e.ThirteenthEligible = dto.ThirteenthEligible;
            e.ThirteenthProrated = dto.ThirteenthProrated;
            e.ApplyAHV = dto.ApplyAHV;
            e.ApplyALV = dto.ApplyALV;
            e.ApplyNBU = dto.ApplyNBU;
            e.ApplyBU = dto.ApplyBU;
            e.ApplyBVG = dto.ApplyBVG;
            e.ApplyFAK = dto.ApplyFAK;
            e.ApplyQST = dto.ApplyQST;
            e.PermitType = dto.PermitType;
            e.ChurchMember = dto.ChurchMember;
            e.Canton = dto.Canton;
            e.WithholdingTaxCode = dto.WithholdingTaxCode;
            e.AHVNumber = dto.AHVNumber;
            e.Krankenkasse = dto.Krankenkasse;
            e.BVGPlan = dto.BVGPlan;
            e.Address = dto.Address;
            e.Zip = dto.Zip;
            e.City = dto.City;
            e.Phone = dto.Phone;
        }
    }
}
