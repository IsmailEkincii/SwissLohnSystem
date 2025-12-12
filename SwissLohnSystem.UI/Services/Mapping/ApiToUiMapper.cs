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
        // =========================
        // Company
        // =========================
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
            new CompanyListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Canton = c.Canton,
                Email = c.Email,
                EmployeeCount = employeeCount
            };

        public static CompanyDetailsDto BuildDetails(CompanyDto company, IEnumerable<EmployeeDto> employees) =>
            new CompanyDetailsDto
            {
                Company = company.ToUi(),
                Employees = employees ?? Enumerable.Empty<EmployeeDto>()
            };

        // =========================
        // Employees
        // =========================
        public static EmployeeListItemDto ToListItem(this EmployeeDto e) => new EmployeeListItemDto
        {
            Id = e.Id,
            CompanyId = e.CompanyId,
            FirstName = $"{e.FirstName} {e.LastName}".Trim(),
            Email = e.Email,
            Phone = e.Phone,
            Position = e.Position,
            Active = e.Active
        };

        // =========================
        // Lohn – Firma aylık liste satırı (CompanyMonthlyLohnDto)
        // =========================
        public static CompanyMonthlyLohnDto ToCompanyMonthlyRow(
            this LohnDto l,
            string? employeeName = null)
            => new CompanyMonthlyLohnDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = employeeName ?? $"#{l.EmployeeId}",
                Month = l.Month,
                Year = l.Year,
                BruttoSalary = l.BruttoSalary,
                NetSalary = l.NetSalary,
                TotalDeductions = l.TotalDeductions,
                IsFinal = l.IsFinal
            };

        public static IEnumerable<CompanyMonthlyLohnDto> ToCompanyMonthlyRows(
            this IEnumerable<LohnDto> loehne,
            IEnumerable<EmployeeDto> employees)
        {
            var nameById = employees.ToDictionary(
                e => e.Id,
                e => $"{e.FirstName} {e.LastName}".Trim()
            );

            foreach (var l in loehne)
            {
                var employeeName = nameById.TryGetValue(l.EmployeeId, out var n)
                    ? n
                    : $"#{l.EmployeeId}";

                yield return l.ToCompanyMonthlyRow(employeeName);
            }
        }

        // =========================
        // Lohn – Detay
        // =========================
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

                // Ek brüt kalemleri
                Bonus = l.Bonus,
                ExtraAllowance = l.ExtraAllowance,
                UnpaidDeduction = l.UnpaidDeduction,
                OtherDeduction = l.OtherDeduction,

                // Aylık çalışma saatleri
                MonthlyHours = l.MonthlyHours,
                MonthlyOvertimeHours = l.MonthlyOvertimeHours,

                CreatedAt = l.CreatedAt,
                IsFinal = l.IsFinal,

                // UI enrichments
                EmployeeName = e is null
                    ? (string?)null
                    : $"{e.FirstName} {e.LastName}".Trim(),
                CompanyId = e?.CompanyId,
                CompanyName = c?.Name,

                // Snapshot parametreler (Lohn tablosundan)
                ApplyAHV = l.ApplyAHV,
                ApplyALV = l.ApplyALV,
                ApplyBVG = l.ApplyBVG,
                ApplyNBU = l.ApplyNBU,
                ApplyBU = l.ApplyBU,
                ApplyFAK = l.ApplyFAK,
                ApplyQST = l.ApplyQST,

                PermitType = l.PermitType,
                Canton = l.Canton,
                ChurchMember = l.ChurchMember,
                WithholdingTaxCode = l.WithholdingTaxCode,

                HolidayRate = l.HolidayRate,
                HolidayEligible = l.HolidayEligible,

                Comment = l.Comment,

                // Arbeitnehmer-Abzüge Snapshot (AHV/ALV/NBU/BVG/QST)
                EmployeeAhvIvEo = l.EmployeeAhvIvEo,
                EmployeeAlv = l.EmployeeAlv,
                EmployeeNbu = l.EmployeeNbu,
                EmployeeBvg = l.EmployeeBvg,
                EmployeeQst = l.EmployeeQst,

                // Şimdilik boş; ileride PayrollItemDto'dan doldurabiliriz
                Items = new List<LohnItemDto>()
            };
    }
}
