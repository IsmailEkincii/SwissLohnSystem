using System;
using System.Collections.Generic;
using System.Linq;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;

namespace SwissLohnSystem.UI.Services.Mapping
{
    /// <summary>API'den gelen DTO’ları UI DTO’larına dönüştürür ve zenginleştirir.</summary>
    public static class ApiToUiMapper
    {
        // Company
        public static CompanyDto ToUi(this CompanyDto api) => new CompanyDto
        {
            Id = api.Id,
            Name = api.Name?.Trim() ?? "",
            Canton = api.Canton?.Trim() ?? "",
            Address = string.IsNullOrWhiteSpace(api.Address) ? null : api.Address!.Trim(),
            Email = string.IsNullOrWhiteSpace(api.Email) ? null : api.Email!.Trim(),
            Phone = string.IsNullOrWhiteSpace(api.Phone) ? null : api.Phone!.Trim(),
            TaxNumber = string.IsNullOrWhiteSpace(api.TaxNumber) ? null : api.TaxNumber!.Trim()
        };

        public static CompanyListItemDto ToListItem(this CompanyDto c, int? employeeCount = null) =>
            new CompanyListItemDto { Id = c.Id, Name = c.Name, Canton = c.Canton, Email = c.Email, EmployeeCount = employeeCount };

        public static CompanyDetailsDto BuildDetails(CompanyDto company, IEnumerable<EmployeeDto> employees) =>
            new CompanyDetailsDto { Company = company.ToUi(), Employees = employees ?? Enumerable.Empty<EmployeeDto>() };

        // Employees
        public static EmployeeListItemDto ToListItem(this EmployeeDto e) => new EmployeeListItemDto
        {
            Id = e.Id,
            CompanyId = e.CompanyId,
            FirstName = $"{e.FirstName} {e.LastName}".Trim(),
            Email = e.Email,
            Position = e.Position,
            Active = e.Active
        };

        // Lohn
        public static LohnMonthlyRowDto ToMonthlyRow(this LohnDto l, string? employeeName = null) =>
            new LohnMonthlyRowDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                Month = l.Month,
                Year = l.Year,
                BruttoSalary = l.BruttoSalary,
                NetSalary = l.NetSalary,
                IsFinal = l.IsFinal,
                EmployeeName = employeeName ?? ""
            };

        public static IEnumerable<LohnMonthlyRowDto> ToMonthlyRows(
            this IEnumerable<LohnDto> loehne,
            IEnumerable<EmployeeDto> employees)
        {
            var nameById = employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
            foreach (var l in loehne)
                yield return l.ToMonthlyRow(nameById.TryGetValue(l.EmployeeId, out var n) ? n : $"#{l.EmployeeId}");
        }

        public static LohnDetailsDto ToDetails(this LohnDto l, EmployeeDto? e = null, CompanyDto? c = null) =>
            new LohnDetailsDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                Month = l.Month,
                Year = l.Year,
                BruttoSalary = l.BruttoSalary,
                TotalDeductions = l.TotalDeductions,
                NetSalary = l.NetSalary,
                ChildAllowance = l.ChildAllowance,
                HolidayAllowance = l.HolidayAllowance,
                OvertimePay = l.OvertimePay,
                CreatedAt = l.CreatedAt,
                IsFinal = l.IsFinal,
                EmployeeName = e is null ? null : $"{e.FirstName} {e.LastName}".Trim(),
                CompanyId = e?.CompanyId,
                CompanyName = c?.Name
            };
    }
}
