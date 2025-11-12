using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Employees; // EmployeeCreateDto, EmployeeDto
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Companies.Employees
{
    public class CreateModel : PageModel, IValidatableObject
    {
        private readonly ApiClient _api;
        public CreateModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)]
        public int CompanyId { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public void OnGet()
        {
            Input.CompanyId = CompanyId;
            Input.Active = true; // default
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CompanyId <= 0)
            {
                TempData["Error"] = "Ungültige Firmen-ID.";
                return RedirectToPage("/Companies/Index");
            }

            if (!ModelState.IsValid) return Page();

            try
            {
                // UI seçimini API beklenen deðere çevir
                var salaryType = Input.SalaryTypeOption == "Stundenlohn" ? "Hourly" : "Monthly";

                var dto = new EmployeeCreateDto
                {
                    CompanyId = Input.CompanyId,
                    FirstName = Input.FirstName!.Trim(),
                    LastName = Input.LastName!.Trim(),
                    Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                    Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position!.Trim(),
                    BirthDate = Input.BirthDate,
                    MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus,
                    ChildCount = Input.ChildCount ?? 0,
                    SalaryType = salaryType,                   // "Monthly" | "Hourly"
                    HourlyRate = Input.HourlyRate ?? 0m,
                    MonthlyHours = Input.MonthlyHours ?? 0,
                    BruttoSalary = Input.BruttoSalary ?? 0m,
                    StartDate = Input.StartDate!.Value,
                    EndDate = Input.EndDate,
                    Active = Input.Active,
                    // Opsiyonel parametreler API DTO’sunda var; UI’de þimdilik boþ býrakýyoruz:
                    PensumPercent = null,
                    HolidayRate = null,
                    OvertimeRate = null,
                    WithholdingTaxCode = null,
                    AHVNumber = null,
                    Krankenkasse = null,
                    BVGPlan = null,
                    Address = null,
                    Zip = null,
                    City = null,
                    Phone = null
                };

                var (ok, data, message) = await _api.PostAsync<EmployeeDto>("/api/Employee", dto);

                if (ok && data is not null)
                {
                    TempData["Toast"] = message ?? "Mitarbeiter wurde erfolgreich hinzugefügt.";
                    return RedirectToPage("/Companies/Details", new { id = CompanyId });
                }

                TempData["Error"] = message ?? "Speichern fehlgeschlagen.";
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Fehler: {ex.Message}";
                return Page();
            }
        }

        // ----- Form Model -----
        public class InputModel
        {
            [Required] public int CompanyId { get; set; }

            [Required(ErrorMessage = "Vorname ist erforderlich.")]
            [StringLength(150, ErrorMessage = "Vorname darf maximal 150 Zeichen lang sein.")]
            public string? FirstName { get; set; }

            [Required(ErrorMessage = "Nachname ist erforderlich.")]
            [StringLength(150, ErrorMessage = "Nachname darf maximal 150 Zeichen lang sein.")]
            public string? LastName { get; set; }

            [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
            public string? Email { get; set; }

            public string? Position { get; set; }
            public DateTime? BirthDate { get; set; }
            public string? MaritalStatus { get; set; } // Single / Married

            [Range(0, 20, ErrorMessage = "Anzahl Kinder muss zwischen 0 und 20 sein.")]
            public int? ChildCount { get; set; }

            [Required(ErrorMessage = "Gehaltsart ist erforderlich.")]
            [RegularExpression("Monatslohn|Stundenlohn", ErrorMessage = "Ungültige Gehaltsart.")]
            public string? SalaryTypeOption { get; set; } // UI seçimi

            [Range(0, 1000, ErrorMessage = "Stundenlohn muss zwischen 0 und 1000 liegen.")]
            public decimal? HourlyRate { get; set; }

            [Range(0, 300, ErrorMessage = "Monatsstunden müssen zwischen 0 und 300 liegen.")]
            public int? MonthlyHours { get; set; }

            [Range(0, 1000000, ErrorMessage = "Bruttolohn muss zwischen 0 und 1'000'000 liegen.")]
            public decimal? BruttoSalary { get; set; }

            [Required(ErrorMessage = "Startdatum ist erforderlich.")]
            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }
            public bool Active { get; set; } = true;
        }

        // ----- Ek sunucu tarafý kontrolleri -----
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Input.StartDate.HasValue && Input.EndDate.HasValue && Input.EndDate < Input.StartDate)
            {
                yield return new ValidationResult("Enddatum darf nicht vor dem Startdatum liegen.", new[] { nameof(Input.EndDate) });
            }

            if (Input.SalaryTypeOption == "Stundenlohn")
            {
                if (Input.HourlyRate is null or <= 0)
                    yield return new ValidationResult("Stundenlohn ist erforderlich (> 0) für Stundenlohn.", new[] { nameof(Input.HourlyRate) });
            }
            else if (Input.SalaryTypeOption == "Monatslohn")
            {
                if (Input.BruttoSalary is null or <= 0)
                    yield return new ValidationResult("Bruttolohn ist erforderlich (> 0) für Monatslohn.", new[] { nameof(Input.BruttoSalary) });
            }
        }
    }
}
