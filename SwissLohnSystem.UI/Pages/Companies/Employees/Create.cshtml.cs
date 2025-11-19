using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Companies.Employees
{
    public class CreateModel : PageModel, IValidatableObject
    {
        private readonly ApiClient _api;
        public CreateModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)]
        public int CompanyId { get; set; }

        /// <summary>
        /// API'deki EmployeeCreateDto ile bire bir aynı DTO.
        /// </summary>
        [BindProperty]
        public EmployeeCreateDto Input { get; set; } = new();

        [TempData] public string? Toast { get; set; }

        public void OnGet(int companyId)
        {
            CompanyId = companyId;

            // Varsayılanları tek yerde dolduralım
            Input = new EmployeeCreateDto
            {
                CompanyId = companyId,
                StartDate = DateTime.Today,
                Active = true,

                // Gehaltsart
                SalaryType = "Monthly",

                // Arbeitszeit
                WeeklyHours = 42,
                PensumPercent = 100m,

                // Kanton / Steuer
                Canton = "ZH",
                PermitType = "B",
                ApplyQST = false,
                ChurchMember = false,

                // Sozialversicherungen (default: hepsi açık)
                ApplyAHV = true,
                ApplyALV = true,
                ApplyNBU = true,
                ApplyBU = true,
                ApplyBVG = true,
                ApplyFAK = true,

                // Lohnparameter default’ları
                HolidayEligible = true,
                ThirteenthEligible = false,
                ThirteenthProrated = false
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Gehaltsart zorunlu
            if (string.IsNullOrWhiteSpace(Input.SalaryType))
            {
                ModelState.AddModelError(nameof(Input.SalaryType), "Gehaltsart ist erforderlich.");
                return Page();
            }

            // Basit AHV format kontrolü
            if (!string.IsNullOrWhiteSpace(Input.AHVNumber))
            {
                var ok = Regex.IsMatch(Input.AHVNumber.Trim(), @"^(756\.\d{4}\.\d{4}\.\d{2}|756\d{10})$");
                if (!ok)
                {
                    ModelState.AddModelError(nameof(Input.AHVNumber), "Ungültige AHV-Nummer. Beispiel: 756.1234.5678.97");
                    return Page();
                }
            }

            // Trim’ler
            Input.FirstName = Input.FirstName?.Trim() ?? "";
            Input.LastName = Input.LastName?.Trim() ?? "";
            Input.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
            Input.Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position.Trim();
            Input.MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus;
            Input.AHVNumber = string.IsNullOrWhiteSpace(Input.AHVNumber) ? null : Input.AHVNumber.Trim();
            Input.Krankenkasse = string.IsNullOrWhiteSpace(Input.Krankenkasse) ? null : Input.Krankenkasse.Trim();
            Input.BVGPlan = string.IsNullOrWhiteSpace(Input.BVGPlan) ? null : Input.BVGPlan.Trim();
            Input.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();
            Input.Zip = string.IsNullOrWhiteSpace(Input.Zip) ? null : Input.Zip.Trim();
            Input.City = string.IsNullOrWhiteSpace(Input.City) ? null : Input.City.Trim();
            Input.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();
            Input.Canton = string.IsNullOrWhiteSpace(Input.Canton)
                ? "ZH"
                : Input.Canton.Trim().ToUpperInvariant();

            // PermitType boş gelirse default B
            if (string.IsNullOrWhiteSpace(Input.PermitType))
                Input.PermitType = "B";

            var (success, createdEmployee, message) =
                await _api.PostAsync<EmployeeDto>("/api/Employee", Input);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message ?? "Erstellung fehlgeschlagen.");
                return Page();
            }

            TempData["Alert"] = $"Mitarbeiter \"{Input.FirstName} {Input.LastName}\" wurde erfolgreich erstellt.";
            return RedirectToPage("/Companies/Details", new { id = Input.CompanyId });
        }

        // Ek UI doğrulamaları (SalaryType/Brutto/Hourly)
        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            if (Input.SalaryType == "Hourly")
            {
                if (Input.HourlyRate <= 0)
                    yield return new ValidationResult(
                        "Stundenlohn ist erforderlich und muss größer als 0 sein.",
                        new[] { nameof(Input.HourlyRate) });
            }
            else if (Input.SalaryType == "Monthly")
            {
                if (Input.BruttoSalary <= 0)
                    yield return new ValidationResult(
                        "Bruttolohn ist erforderlich und muss größer als 0 sein.",
                        new[] { nameof(Input.BruttoSalary) });
            }

            if (Input.EndDate.HasValue && Input.EndDate.Value < Input.StartDate)
            {
                yield return new ValidationResult(
                    "Enddatum darf nicht vor dem Startdatum liegen.",
                    new[] { nameof(Input.EndDate) });
            }
        }
    }
}
