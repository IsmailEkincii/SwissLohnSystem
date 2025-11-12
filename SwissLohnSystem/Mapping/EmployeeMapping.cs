using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class EmployeeMappings
    {
        public static EmployeeDto ToDto(this Employee e) =>
            new(
                e.Id, e.CompanyId,
                e.FirstName, e.LastName,
                e.Email, e.Position,
                e.BirthDate, e.MaritalStatus, e.ChildCount,
                e.SalaryType, e.HourlyRate, e.MonthlyHours, e.BruttoSalary,
                e.StartDate, e.EndDate, e.Active,
                e.AHVNumber, e.Krankenkasse, e.BVGPlan,
                e.PensumPercent, e.HolidayRate, e.OvertimeRate, e.WithholdingTaxCode,
                e.Address, e.Zip, e.City, e.Phone
            );

        public static Employee ToEntity(this EmployeeCreateDto dto) => new()
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
            AHVNumber = string.IsNullOrWhiteSpace(dto.AHVNumber) ? null : dto.AHVNumber.Trim(),
            Krankenkasse = string.IsNullOrWhiteSpace(dto.Krankenkasse) ? null : dto.Krankenkasse.Trim(),
            BVGPlan = string.IsNullOrWhiteSpace(dto.BVGPlan) ? null : dto.BVGPlan.Trim(),
            PensumPercent = dto.PensumPercent ?? 100m,
            HolidayRate = dto.HolidayRate ?? 8.33m,
            OvertimeRate = dto.OvertimeRate ?? 1.25m,
            WithholdingTaxCode = string.IsNullOrWhiteSpace(dto.WithholdingTaxCode) ? null : dto.WithholdingTaxCode.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            Zip = string.IsNullOrWhiteSpace(dto.Zip) ? null : dto.Zip.Trim(),
            City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim(),
            Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim()
        };

        public static void Apply(this Employee entity, EmployeeUpdateDto dto)
        {
            entity.CompanyId = dto.CompanyId;
            entity.FirstName = dto.FirstName.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            entity.Position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim();
            entity.BirthDate = dto.BirthDate;
            entity.MaritalStatus = string.IsNullOrWhiteSpace(dto.MaritalStatus) ? null : dto.MaritalStatus.Trim();
            entity.ChildCount = dto.ChildCount;
            entity.SalaryType = dto.SalaryType;
            entity.HourlyRate = dto.HourlyRate;
            entity.MonthlyHours = dto.MonthlyHours;
            entity.BruttoSalary = dto.BruttoSalary;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Active = dto.Active;
            entity.AHVNumber = string.IsNullOrWhiteSpace(dto.AHVNumber) ? null : dto.AHVNumber.Trim();
            entity.Krankenkasse = string.IsNullOrWhiteSpace(dto.Krankenkasse) ? null : dto.Krankenkasse.Trim();
            entity.BVGPlan = string.IsNullOrWhiteSpace(dto.BVGPlan) ? null : dto.BVGPlan.Trim();
            entity.PensumPercent = dto.PensumPercent ?? entity.PensumPercent;
            entity.HolidayRate = dto.HolidayRate ?? entity.HolidayRate;
            entity.OvertimeRate = dto.OvertimeRate ?? entity.OvertimeRate;
            entity.WithholdingTaxCode = string.IsNullOrWhiteSpace(dto.WithholdingTaxCode) ? null : dto.WithholdingTaxCode.Trim();
            entity.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            entity.Zip = string.IsNullOrWhiteSpace(dto.Zip) ? null : dto.Zip.Trim();
            entity.City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City.Trim();
            entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        }
    }
}
