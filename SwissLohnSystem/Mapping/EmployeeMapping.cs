using System;
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

            Gender = e.Gender,          // ✅ "M" | "F" | "X" | null
            ApplyKTG = e.ApplyKTG,      // ✅

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

        public static Employee ToEntity(this EmployeeCreateDto dto)
        {
            var gender = NormalizeGender(dto.Gender); // ✅ validate here

            return new Employee
            {
                CompanyId = dto.CompanyId,

                FirstName = (dto.FirstName ?? "").Trim(),
                LastName = (dto.LastName ?? "").Trim(),
                Email = Clean(dto.Email),
                Position = Clean(dto.Position),

                BirthDate = dto.BirthDate,
                MaritalStatus = Clean(dto.MaritalStatus),
                ChildCount = dto.ChildCount,

                Gender = gender,          // ✅ null or M/F/X
                ApplyKTG = dto.ApplyKTG,  // ✅

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

                PermitType = NormalizePermit(dto.PermitType),
                ChurchMember = dto.ChurchMember,
                Canton = NormalizeCanton(dto.Canton),
                WithholdingTaxCode = Clean(dto.WithholdingTaxCode),

                AHVNumber = Clean(dto.AHVNumber),
                Krankenkasse = Clean(dto.Krankenkasse),
                BVGPlan = Clean(dto.BVGPlan),

                Address = Clean(dto.Address),
                Zip = Clean(dto.Zip),
                City = Clean(dto.City),
                Phone = Clean(dto.Phone)
            };
        }

        public static void Apply(this Employee e, EmployeeUpdateDto dto)
        {
            var gender = NormalizeGender(dto.Gender); // ✅ validate here (null allowed)

            e.CompanyId = dto.CompanyId;

            e.FirstName = (dto.FirstName ?? "").Trim();
            e.LastName = (dto.LastName ?? "").Trim();
            e.Email = Clean(dto.Email);
            e.Position = Clean(dto.Position);

            e.BirthDate = dto.BirthDate;
            e.MaritalStatus = Clean(dto.MaritalStatus);
            e.ChildCount = dto.ChildCount;

            // ✅ if dto.Gender null => keep existing; if invalid => exception above
            if (dto.Gender is not null)
                e.Gender = gender;

            e.ApplyKTG = dto.ApplyKTG;

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

            if (dto.PermitType is not null)
                e.PermitType = NormalizePermit(dto.PermitType);

            e.ChurchMember = dto.ChurchMember;

            if (dto.Canton is not null)
                e.Canton = NormalizeCanton(dto.Canton);

            e.WithholdingTaxCode = Clean(dto.WithholdingTaxCode);

            e.AHVNumber = Clean(dto.AHVNumber);
            e.Krankenkasse = Clean(dto.Krankenkasse);
            e.BVGPlan = Clean(dto.BVGPlan);

            e.Address = Clean(dto.Address);
            e.Zip = Clean(dto.Zip);
            e.City = Clean(dto.City);
            e.Phone = Clean(dto.Phone);
        }

        // =========================
        // Helpers
        // =========================

        private static string? Clean(string? s)
            => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        private static string NormalizePermit(string? p)
            => string.IsNullOrWhiteSpace(p) ? "B" : p.Trim().ToUpperInvariant();

        private static string NormalizeCanton(string? c)
            => string.IsNullOrWhiteSpace(c) ? "ZH" : c.Trim().ToUpperInvariant();

        private static string? NormalizeGender(string? g)
        {
            if (g is null) return null; // null allowed

            if (string.IsNullOrWhiteSpace(g))
                return null;

            g = g.Trim().ToUpperInvariant();

            return g switch
            {
                "M" => "M",
                "F" => "F",
                "X" => "X",
                _ => throw new ArgumentException("Gender must be M, F, or X.")
            };
        }
    }
}
