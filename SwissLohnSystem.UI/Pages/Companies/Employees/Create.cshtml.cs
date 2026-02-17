using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Payroll;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Lookups;

namespace SwissLohnSystem.UI.Pages.Companies.Employees
{
    public class CreateModel : PageModel, IValidatableObject
    {
        private readonly ApiClient _api;
        public CreateModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)]
        public int CompanyId { get; set; }

        [BindProperty]
        public EmployeeCreateDto Input { get; set; } = new();

        [BindProperty]
        public bool UseCompanyBvgPlan { get; set; } = true;

        public List<SelectListItem> PermitTypes { get; private set; } = new();
        public List<SelectListItem> BvgPlans { get; private set; } = new();

        public string ApiBaseUrl => _api.BaseUrl?.TrimEnd('/') ?? string.Empty;

        public async Task OnGetAsync(int companyId)
        {
            CompanyId = companyId;
            ViewData["ApiBaseUrl"] = ApiBaseUrl;

            Input = new EmployeeCreateDto
            {
                CompanyId = companyId,
                StartDate = DateTime.Today,
                Active = true,

                // defaultlar
                SalaryType = "Monthly",
                WeeklyHours = 42,
                PensumPercent = 100m,

                Canton = "ZH",
                PermitType = "B",
                ApplyQST = false,
                ChurchMember = false,

                ApplyAHV = true,
                ApplyALV = true,
                ApplyNBU = true,
                ApplyBU = true,
                ApplyBVG = true,
                ApplyFAK = true,

                HolidayEligible = true,
                ThirteenthEligible = false,
                ThirteenthProrated = false,

                // Gender boş kalsın (UI’da seçtireceğiz)
                Gender = null
            };

            UseCompanyBvgPlan = true;
            Input.BVGPlan = null;

            PermitTypes = QstUiLookups.GetPermitTypes();
            await LoadBvgPlansAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ApiBaseUrl"] = ApiBaseUrl;

            PermitTypes = QstUiLookups.GetPermitTypes();
            await LoadBvgPlansAsync();

            if (!ModelState.IsValid)
                return Page();

            // AHV format
            if (!string.IsNullOrWhiteSpace(Input.AHVNumber))
            {
                var ok = Regex.IsMatch(Input.AHVNumber.Trim(), @"^(756\.\d{4}\.\d{4}\.\d{2}|756\d{10})$");
                if (!ok)
                {
                    ModelState.AddModelError(nameof(Input.AHVNumber), "Ungültige AHV-Nummer. Beispiel: 756.1234.5678.97");
                    return Page();
                }
            }

            // QST kapalıysa code null
            if (!Input.ApplyQST)
                Input.WithholdingTaxCode = null;

            // BVG: Firmen-Standard mı?
            if (UseCompanyBvgPlan)
                Input.BVGPlan = null;
            else
                Input.BVGPlan = string.IsNullOrWhiteSpace(Input.BVGPlan) ? null : Input.BVGPlan.Trim();

            // trims
            Input.FirstName = (Input.FirstName ?? "").Trim();
            Input.LastName = (Input.LastName ?? "").Trim();

            Input.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
            Input.Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position.Trim();
            Input.MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus.Trim();

            Input.AHVNumber = string.IsNullOrWhiteSpace(Input.AHVNumber) ? null : Input.AHVNumber.Trim();
            Input.Krankenkasse = string.IsNullOrWhiteSpace(Input.Krankenkasse) ? null : Input.Krankenkasse.Trim();

            Input.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();
            Input.Zip = string.IsNullOrWhiteSpace(Input.Zip) ? null : Input.Zip.Trim();
            Input.City = string.IsNullOrWhiteSpace(Input.City) ? null : Input.City.Trim();
            Input.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();

            Input.Canton = string.IsNullOrWhiteSpace(Input.Canton) ? "ZH" : Input.Canton.Trim().ToUpperInvariant();
            Input.PermitType = string.IsNullOrWhiteSpace(Input.PermitType) ? "B" : Input.PermitType.Trim().ToUpperInvariant();

            // Gender normalize (UI tarafı)
            if (!string.IsNullOrWhiteSpace(Input.Gender))
                Input.Gender = Input.Gender.Trim().ToUpperInvariant();

            // QST açık ise validation
            if (Input.ApplyQST)
            {
                if (string.IsNullOrWhiteSpace(Input.PermitType))
                    ModelState.AddModelError(nameof(Input.PermitType), "Bitte eine Bewilligung auswählen.");

                if (string.IsNullOrWhiteSpace(Input.WithholdingTaxCode))
                    ModelState.AddModelError(nameof(Input.WithholdingTaxCode), "Bitte einen Quellensteuer-Tarif auswählen.");

                if (!ModelState.IsValid)
                    return Page();
            }

            var (success, createdEmployee, message) =
                await _api.PostAsync<EmployeeDto>("/api/Employee", Input);

            if (!success || createdEmployee is null)
            {
                TempData["Error"] = message ?? "Erstellung fehlgeschlagen.";
                return Page();
            }

            TempData["Toast"] = $"Mitarbeiter \"{createdEmployee.FirstName} {createdEmployee.LastName}\" wurde erstellt.";
            return RedirectToPage("/Companies/Details", new { id = Input.CompanyId });
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
            // Salary validation
            if (Input.SalaryType == "Hourly")
            {
                if (Input.HourlyRate <= 0)
                    yield return new ValidationResult("Stundenlohn ist erforderlich und muss größer als 0 sein.", new[] { nameof(Input.HourlyRate) });
            }
            else if (Input.SalaryType == "Monthly")
            {
                if (Input.BruttoSalary <= 0)
                    yield return new ValidationResult("Bruttolohn ist erforderlich und muss größer als 0 sein.", new[] { nameof(Input.BruttoSalary) });
            }

            // Date validation
            if (Input.EndDate.HasValue && Input.EndDate.Value < Input.StartDate)
                yield return new ValidationResult("Enddatum darf nicht vor dem Startdatum liegen.", new[] { nameof(Input.EndDate) });

            // Gender validation (optional field)
            if (!string.IsNullOrWhiteSpace(Input.Gender))
            {
                var g = Input.Gender.Trim().ToUpperInvariant();
                if (g != "M" && g != "F" && g != "X")
                    yield return new ValidationResult("Geschlecht muss M, F oder X sein.", new[] { nameof(Input.Gender) });
            }
        }

        private async Task LoadBvgPlansAsync()
        {
            var (ok, data, msg) = await _api.GetAsync<List<BvgPlanListItemDto>>("/api/Settings/bvg-plans");

            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- bitte wählen --" }
            };

            if (ok && data is not null)
            {
                foreach (var p in data)
                {
                    var text = p.Code;
                    if (p.Year.HasValue) text = $"{p.Code} ({p.Year})";
                    list.Add(new SelectListItem { Value = p.Code, Text = text });
                }
            }

            BvgPlans = list;
        }
    }
}
