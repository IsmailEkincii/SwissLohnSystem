using SwissLohnSystem.API.Data;            // Employee entity burada varsayıyorum
using SwissLohnSystem.API.DTOs.Employees;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class EmployeeMapping
    {
        // ENTITY -> DTO
        public static EmployeeDto ToDto(this Employee e)
        {
            return new EmployeeDto
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
        }

        // CREATE DTO -> ENTITY
        public static Employee ToEntity(this EmployeeCreateDto dto)
        {
            return new Employee
            {
                CompanyId = dto.CompanyId,

                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email,
                Position = dto.Position,

                BirthDate = dto.BirthDate,
                MaritalStatus = dto.MaritalStatus,
                ChildCount = dto.ChildCount,

                SalaryType = dto.SalaryType,
                HourlyRate = dto.HourlyRate,
                MonthlyHours = dto.MonthlyHours,
                BruttoSalary = dto.BruttoSalary,

                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Active = dto.Active,

                AHVNumber = dto.AHVNumber,
                Krankenkasse = dto.Krankenkasse,
                BVGPlan = dto.BVGPlan,

                PensumPercent = dto.PensumPercent,
                HolidayRate = dto.HolidayRate,
                OvertimeRate = dto.OvertimeRate,
                WithholdingTaxCode = dto.WithholdingTaxCode,

                WeeklyHours = dto.WeeklyHours,
                ApplyAHV = dto.ApplyAHV,
                ApplyALV = dto.ApplyALV,
                ApplyNBU = dto.ApplyNBU,
                ApplyBU = dto.ApplyBU,
                ApplyBVG = dto.ApplyBVG,
                ApplyFAK = dto.ApplyFAK,
                ApplyQST = dto.ApplyQST,

                HolidayEligible = dto.HolidayEligible,
                ThirteenthEligible = dto.ThirteenthEligible,
                ThirteenthProrated = dto.ThirteenthProrated,

                PermitType = dto.PermitType,
                ChurchMember = dto.ChurchMember,
                Canton = dto.Canton,

                Address = dto.Address,
                Zip = dto.Zip,
                City = dto.City,
                Phone = dto.Phone
            };
        }

        // UPDATE DTO -> EXISTING ENTITY
        public static void Apply(this Employee entity, EmployeeUpdateDto dto)
        {
            entity.CompanyId = dto.CompanyId;

            entity.FirstName = dto.FirstName.Trim();
            entity.LastName = dto.LastName.Trim();
            entity.Email = dto.Email;
            entity.Position = dto.Position;

            entity.BirthDate = dto.BirthDate;
            entity.MaritalStatus = dto.MaritalStatus;
            entity.ChildCount = dto.ChildCount;

            entity.SalaryType = dto.SalaryType;
            entity.HourlyRate = dto.HourlyRate;
            entity.MonthlyHours = dto.MonthlyHours;
            entity.BruttoSalary = dto.BruttoSalary;

            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.Active = dto.Active;

            entity.AHVNumber = dto.AHVNumber;
            entity.Krankenkasse = dto.Krankenkasse;
            entity.BVGPlan = dto.BVGPlan;

            entity.PensumPercent = dto.PensumPercent;
            entity.HolidayRate = dto.HolidayRate;
            entity.OvertimeRate = dto.OvertimeRate;
            entity.WithholdingTaxCode = dto.WithholdingTaxCode;

            entity.WeeklyHours = dto.WeeklyHours;
            entity.ApplyAHV = dto.ApplyAHV;
            entity.ApplyALV = dto.ApplyALV;
            entity.ApplyNBU = dto.ApplyNBU;
            entity.ApplyBU = dto.ApplyBU;
            entity.ApplyBVG = dto.ApplyBVG;
            entity.ApplyFAK = dto.ApplyFAK;
            entity.ApplyQST = dto.ApplyQST;

            entity.HolidayEligible = dto.HolidayEligible;
            entity.ThirteenthEligible = dto.ThirteenthEligible;
            entity.ThirteenthProrated = dto.ThirteenthProrated;

            entity.PermitType = dto.PermitType;
            entity.ChurchMember = dto.ChurchMember;
            entity.Canton = dto.Canton;

            entity.Address = dto.Address;
            entity.Zip = dto.Zip;
            entity.City = dto.City;
            entity.Phone = dto.Phone;
        }
    }
}
