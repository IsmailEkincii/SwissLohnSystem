using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.Services;


namespace SwissLohnSystem.UI.Pages.Employees
{
    public class EditModel : PageModel, IValidatableObject
    {
        private readonly ApiClient _api;
        public EditModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        [BindProperty(SupportsGet = true)] public int CompanyId { get; set; }

        [BindProperty] public InputModel Input { get; set; } = new();

        [TempData] public string? Toast { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            var (ok, emp, msg) = await _api.GetAsync<EmployeeDtoFromApi>($"/api/Employee/{id}");
            if (!ok || emp is null)
            {
                TempData["Error"] = msg ?? "Mitarbeiter wurde nicht gefunden.";
                return RedirectToPage("/Companies/Index");
            }

            CompanyId = emp.CompanyId;

            Input = new InputModel
            {
                Id = emp.Id,
                CompanyId = emp.CompanyId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                Position = emp.Position,
                BirthDate = emp.BirthDate,
                MaritalStatus = emp.MaritalStatus,
                ChildCount = emp.ChildCount,
                SalaryTypeOption = emp.SalaryType == "Hourly" ? "Stundenlohn" : "Monatslohn",
                HourlyRate = emp.HourlyRate,
                MonthlyHours = emp.MonthlyHours,
                BruttoSalary = emp.BruttoSalary,
                StartDate = emp.StartDate,
                EndDate = emp.EndDate,
                Active = emp.Active,

                // Neue Payroll-Felder
                WeeklyHours = emp.WeeklyHours,
                PensumPercent = emp.PensumPercent,
                HolidayRate = emp.HolidayRate,
                OvertimeRate = emp.OvertimeRate,
                WithholdingTaxCode = emp.WithholdingTaxCode,

                AHVNumber = emp.AHVNumber,
                Krankenkasse = emp.Krankenkasse,
                BVGPlan = emp.BVGPlan,

                Address = emp.Address,
                Zip = emp.Zip,
                City = emp.City,
                Phone = emp.Phone,

                ApplyAHV = emp.ApplyAHV,
                ApplyALV = emp.ApplyALV,
                ApplyNBU = emp.ApplyNBU,
                ApplyBU = emp.ApplyBU,
                ApplyBVG = emp.ApplyBVG,
                ApplyFAK = emp.ApplyFAK,
                ApplyQST = emp.ApplyQST,

                HolidayEligible = emp.HolidayEligible,
                ThirteenthEligible = emp.ThirteenthEligible,
                ThirteenthProrated = emp.ThirteenthProrated,

                PermitType = emp.PermitType,
                ChurchMember = emp.ChurchMember,
                Canton = emp.Canton
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var salaryType = Input.SalaryTypeOption == "Stundenlohn" ? "Hourly" : "Monthly";

            // Basit AHV format kontrolü (UI tarafýnda da kullanýcýya geri bildirim)
            if (!string.IsNullOrWhiteSpace(Input.AHVNumber))
            {
                var ok = Regex.IsMatch(Input.AHVNumber.Trim(), @"^(756\.\d{4}\.\d{4}\.\d{2}|756\d{10})$");
                if (!ok)
                {
                    ModelState.AddModelError(nameof(Input.AHVNumber), "Ungültige AHV-Nummer. Beispiel: 756.1234.5678.97");
                    return Page();
                }
            }

            var body = new EmployeeUpdateDtoForApi
            {
                Id = Input.Id,
                CompanyId = Input.CompanyId,
                FirstName = Input.FirstName!.Trim(),
                LastName = Input.LastName!.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position!.Trim(),
                BirthDate = Input.BirthDate,
                MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus!,
                ChildCount = Input.ChildCount ?? 0,
                SalaryType = salaryType,
                HourlyRate = Input.HourlyRate ?? 0m,
                MonthlyHours = Input.MonthlyHours ?? 0,
                BruttoSalary = Input.BruttoSalary ?? 0m,
                StartDate = Input.StartDate!.Value,
                EndDate = Input.EndDate,
                Active = Input.Active,

                AHVNumber = string.IsNullOrWhiteSpace(Input.AHVNumber) ? null : Input.AHVNumber.Trim(),
                Krankenkasse = string.IsNullOrWhiteSpace(Input.Krankenkasse) ? null : Input.Krankenkasse.Trim(),
                BVGPlan = string.IsNullOrWhiteSpace(Input.BVGPlan) ? null : Input.BVGPlan.Trim(),

                PensumPercent = Input.PensumPercent,
                HolidayRate = Input.HolidayRate,
                OvertimeRate = Input.OvertimeRate,
                WithholdingTaxCode = string.IsNullOrWhiteSpace(Input.WithholdingTaxCode)
                    ? null
                    : Input.WithholdingTaxCode.Trim(),

                Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim(),
                Zip = string.IsNullOrWhiteSpace(Input.Zip) ? null : Input.Zip.Trim(),
                City = string.IsNullOrWhiteSpace(Input.City) ? null : Input.City.Trim(),
                Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim(),

                WeeklyHours = Input.WeeklyHours ?? 0,
                ApplyAHV = Input.ApplyAHV,
                ApplyALV = Input.ApplyALV,
                ApplyNBU = Input.ApplyNBU,
                ApplyBU = Input.ApplyBU,
                ApplyBVG = Input.ApplyBVG,
                ApplyFAK = Input.ApplyFAK,
                ApplyQST = Input.ApplyQST,

                HolidayEligible = Input.HolidayEligible,
                ThirteenthEligible = Input.ThirteenthEligible,
                ThirteenthProrated = Input.ThirteenthProrated,

                PermitType = string.IsNullOrWhiteSpace(Input.PermitType) ? "B" : Input.PermitType.Trim(),
                ChurchMember = Input.ChurchMember,
                Canton = string.IsNullOrWhiteSpace(Input.Canton) ? "ZH" : Input.Canton.Trim()
            };

            var (success, _, message) = await _api.PutAsync<string>($"/api/Employee/{Input.Id}", body);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message ?? "Aktualisierung fehlgeschlagen.");
                return Page();
            }

            TempData["Alert"] = "Mitarbeiter erfolgreich aktualisiert.";
            return RedirectToPage("/Companies/Details", new { id = Input.CompanyId });
        }

        // ----- ViewModel -----
        public class InputModel
        {
            [Required] public int Id { get; set; }
            [Required] public int CompanyId { get; set; }

            [Required, StringLength(150)] public string? FirstName { get; set; }
            [Required, StringLength(150)] public string? LastName { get; set; }
            [EmailAddress] public string? Email { get; set; }
            public string? Position { get; set; }

            public DateTime? BirthDate { get; set; }
            public string? MaritalStatus { get; set; }
            [Range(0, 20)] public int? ChildCount { get; set; }

            [Required, RegularExpression("Monatslohn|Stundenlohn")]
            public string? SalaryTypeOption { get; set; }

            [Range(0, 1000)] public decimal? HourlyRate { get; set; }
            [Range(0, 300)] public int? MonthlyHours { get; set; }
            [Range(0, 1_000_000)] public decimal? BruttoSalary { get; set; }

            [Required] public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool Active { get; set; }

            // Payroll / Arbeitszeit
            [Range(0, 80)] public int? WeeklyHours { get; set; }
            [Range(0, 100)] public decimal? PensumPercent { get; set; }
            [Range(0, 100)] public decimal? HolidayRate { get; set; }
            [Range(0, 10)] public decimal? OvertimeRate { get; set; }

            public bool HolidayEligible { get; set; }
            public bool ThirteenthEligible { get; set; }
            public bool ThirteenthProrated { get; set; }

            // Sozialversicherungen Flags
            public bool ApplyAHV { get; set; }
            public bool ApplyALV { get; set; }
            public bool ApplyNBU { get; set; }
            public bool ApplyBU { get; set; }
            public bool ApplyBVG { get; set; }
            public bool ApplyFAK { get; set; }
            public bool ApplyQST { get; set; }

            // Steuer / Kanton
            public string? PermitType { get; set; }
            public bool ChurchMember { get; set; }
            [StringLength(2)] public string? Canton { get; set; }
            public string? WithholdingTaxCode { get; set; }

            // Sozialversicherung
            public string? AHVNumber { get; set; }
            public string? Krankenkasse { get; set; }
            public string? BVGPlan { get; set; }

            // Adresse & Kontakt
            public string? Address { get; set; }
            public string? Zip { get; set; }
            public string? City { get; set; }
            public string? Phone { get; set; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            if (Input.SalaryTypeOption == "Stundenlohn")
            {
                if (Input.HourlyRate is null or <= 0)
                    yield return new ValidationResult(
                        "Stundenlohn ist erforderlich und muss größer als 0 sein.",
                        new[] { nameof(Input.HourlyRate) });
            }
            else if (Input.SalaryTypeOption == "Monatslohn")
            {
                if (Input.BruttoSalary is null or <= 0)
                    yield return new ValidationResult(
                        "Bruttolohn ist erforderlich und muss größer als 0 sein.",
                        new[] { nameof(Input.BruttoSalary) });
            }

            if (Input.EndDate.HasValue && Input.StartDate.HasValue && Input.EndDate < Input.StartDate)
                yield return new ValidationResult(
                    "Enddatum darf nicht vor dem Startdatum liegen.",
                    new[] { nameof(Input.EndDate) });
        }

        // ----- API DTO’larý -----
        public record EmployeeDtoFromApi(
            int Id, int CompanyId,
            string FirstName, string LastName,
            string? Email, string? Position,
            DateTime? BirthDate, string? MaritalStatus, int ChildCount,
            string SalaryType, decimal HourlyRate, int MonthlyHours, decimal BruttoSalary,
            DateTime StartDate, DateTime? EndDate, bool Active,
            string? AHVNumber, string? Krankenkasse, string? BVGPlan,
            decimal? PensumPercent, decimal? HolidayRate, decimal? OvertimeRate, string? WithholdingTaxCode,
            int WeeklyHours,
            bool ApplyAHV, bool ApplyALV, bool ApplyNBU, bool ApplyBU, bool ApplyBVG, bool ApplyFAK, bool ApplyQST,
            bool HolidayEligible, bool ThirteenthEligible, bool ThirteenthProrated,
            string PermitType, bool ChurchMember, string Canton,
            string? Address, string? Zip, string? City, string? Phone
        );

        public class EmployeeUpdateDtoForApi
        {
            public int Id { get; set; }
            public int CompanyId { get; set; }
            public string FirstName { get; set; } = null!;
            public string LastName { get; set; } = null!;
            public string? Email { get; set; }
            public string? Position { get; set; }
            public DateTime? BirthDate { get; set; }
            public string? MaritalStatus { get; set; }
            public int ChildCount { get; set; }
            public string SalaryType { get; set; } = null!; // "Monthly" | "Hourly"
            public decimal HourlyRate { get; set; }
            public int MonthlyHours { get; set; }
            public decimal BruttoSalary { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool Active { get; set; }

            public string? AHVNumber { get; set; }
            public string? Krankenkasse { get; set; }
            public string? BVGPlan { get; set; }
            public decimal? PensumPercent { get; set; }
            public decimal? HolidayRate { get; set; }
            public decimal? OvertimeRate { get; set; }
            public string? WithholdingTaxCode { get; set; }

            public int WeeklyHours { get; set; }
            public bool ApplyAHV { get; set; }
            public bool ApplyALV { get; set; }
            public bool ApplyNBU { get; set; }
            public bool ApplyBU { get; set; }
            public bool ApplyBVG { get; set; }
            public bool ApplyFAK { get; set; }
            public bool ApplyQST { get; set; }

            public bool HolidayEligible { get; set; }
            public bool ThirteenthEligible { get; set; }
            public bool ThirteenthProrated { get; set; }

            public string PermitType { get; set; } = "B";
            public bool ChurchMember { get; set; }
            public string Canton { get; set; } = "ZH";

            public string? Address { get; set; }
            public string? Zip { get; set; }
            public string? City { get; set; }
            public string? Phone { get; set; }
        }
    }
}
