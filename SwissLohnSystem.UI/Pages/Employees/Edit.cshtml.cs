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

namespace SwissLohnSystem.UI.Pages.Employees
{
    public class EditModel : PageModel, IValidatableObject
    {
        private readonly ApiClient _api;
        public EditModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        [BindProperty(SupportsGet = true)] public int CompanyId { get; set; }

        [BindProperty]
        public EmployeeEditDto Input { get; set; } = new();

        [BindProperty]
        public bool UseCompanyBvgPlan { get; set; }

        [TempData] public string? Toast { get; set; }

        public List<SelectListItem> PermitTypes { get; private set; } = new();
        public List<SelectListItem> QstTariffCodes { get; private set; } = new();
        public List<SelectListItem> BvgPlans { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            var (ok, emp, msg) = await _api.GetAsync<EmployeeDto>($"/api/Employee/{id}");
            if (!ok || emp is null)
            {
                TempData["Error"] = msg ?? "Mitarbeiter wurde nicht gefunden.";
                return RedirectToPage("/Companies/Index");
            }

            CompanyId = emp.CompanyId;

            Input = new EmployeeEditDto
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

                SalaryType = emp.SalaryType,
                HourlyRate = emp.HourlyRate,
                MonthlyHours = emp.MonthlyHours,
                BruttoSalary = emp.BruttoSalary,

                StartDate = emp.StartDate,
                EndDate = emp.EndDate,
                Active = emp.Active,

                AHVNumber = emp.AHVNumber,
                Krankenkasse = emp.Krankenkasse,
                BVGPlan = emp.BVGPlan,

                WeeklyHours = emp.WeeklyHours,
                PensumPercent = emp.PensumPercent,
                HolidayRate = emp.HolidayRate,
                OvertimeRate = emp.OvertimeRate,

                HolidayEligible = emp.HolidayEligible,
                ThirteenthEligible = emp.ThirteenthEligible,
                ThirteenthProrated = emp.ThirteenthProrated,

                ApplyAHV = emp.ApplyAHV,
                ApplyALV = emp.ApplyALV,
                ApplyNBU = emp.ApplyNBU,
                ApplyBU = emp.ApplyBU,
                ApplyBVG = emp.ApplyBVG,
                ApplyFAK = emp.ApplyFAK,
                ApplyQST = emp.ApplyQST,

                PermitType = emp.PermitType,
                ChurchMember = emp.ChurchMember,
                Canton = emp.Canton,
                WithholdingTaxCode = emp.WithholdingTaxCode,

                Address = emp.Address,
                Zip = emp.Zip,
                City = emp.City,
                Phone = emp.Phone
            };

            PermitTypes = QstUiLookups.GetPermitTypes();
            await LoadQstTariffsAsync(Input.Canton);

            await LoadBvgPlansAsync();

            UseCompanyBvgPlan = string.IsNullOrWhiteSpace(Input.BVGPlan);
            if (UseCompanyBvgPlan)
                Input.BVGPlan = null;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PermitTypes = QstUiLookups.GetPermitTypes();
            await LoadQstTariffsAsync(Input.Canton);
            await LoadBvgPlansAsync();

            if (!ModelState.IsValid)
                return Page();

            if (string.IsNullOrWhiteSpace(Input.SalaryType))
            {
                ModelState.AddModelError(nameof(Input.SalaryType), "Gehaltsart ist erforderlich.");
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.AHVNumber))
            {
                var ok = Regex.IsMatch(Input.AHVNumber.Trim(), @"^(756\.\d{4}\.\d{4}\.\d{2}|756\d{10})$");
                if (!ok)
                {
                    ModelState.AddModelError(nameof(Input.AHVNumber), "Ungültige AHV-Nummer. Beispiel: 756.1234.5678.97");
                    return Page();
                }
            }

            // ✅ BVG: Firmen-Standard mı, özel plan mı?
            if (UseCompanyBvgPlan)
                Input.BVGPlan = null;
            else
                Input.BVGPlan = string.IsNullOrWhiteSpace(Input.BVGPlan) ? null : Input.BVGPlan.Trim();

            Input.FirstName = Input.FirstName?.Trim() ?? "";
            Input.LastName = Input.LastName?.Trim() ?? "";
            Input.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
            Input.Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position.Trim();
            Input.MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus;
            Input.AHVNumber = string.IsNullOrWhiteSpace(Input.AHVNumber) ? null : Input.AHVNumber.Trim();
            Input.Krankenkasse = string.IsNullOrWhiteSpace(Input.Krankenkasse) ? null : Input.Krankenkasse.Trim();

            Input.Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim();
            Input.Zip = string.IsNullOrWhiteSpace(Input.Zip) ? null : Input.Zip.Trim();
            Input.City = string.IsNullOrWhiteSpace(Input.City) ? null : Input.City.Trim();
            Input.Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim();

            Input.Canton = string.IsNullOrWhiteSpace(Input.Canton) ? "ZH" : Input.Canton.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(Input.PermitType))
                Input.PermitType = "B";

            if (Input.ApplyQST)
            {
                if (string.IsNullOrWhiteSpace(Input.PermitType))
                    ModelState.AddModelError(nameof(Input.PermitType), "Bitte eine Bewilligung auswählen.");

                if (string.IsNullOrWhiteSpace(Input.WithholdingTaxCode))
                    ModelState.AddModelError(nameof(Input.WithholdingTaxCode), "Bitte einen Quellensteuer-Tarif auswählen.");

                if (!ModelState.IsValid)
                    return Page();
            }

            var body = new EmployeeUpdateDto
            {
                Id = Input.Id,
                CompanyId = Input.CompanyId,

                FirstName = Input.FirstName!,
                LastName = Input.LastName!,
                Email = Input.Email,
                Position = Input.Position,

                BirthDate = Input.BirthDate,
                MaritalStatus = Input.MaritalStatus,
                ChildCount = Input.ChildCount,

                SalaryType = Input.SalaryType,
                HourlyRate = Input.HourlyRate,
                MonthlyHours = Input.MonthlyHours,
                BruttoSalary = Input.BruttoSalary,

                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                Active = Input.Active,

                AHVNumber = Input.AHVNumber,
                Krankenkasse = Input.Krankenkasse,
                BVGPlan = Input.BVGPlan,

                WeeklyHours = Input.WeeklyHours,
                PensumPercent = Input.PensumPercent,
                HolidayRate = Input.HolidayRate,
                OvertimeRate = Input.OvertimeRate,

                HolidayEligible = Input.HolidayEligible,
                ThirteenthEligible = Input.ThirteenthEligible,
                ThirteenthProrated = Input.ThirteenthProrated,

                ApplyAHV = Input.ApplyAHV,
                ApplyALV = Input.ApplyALV,
                ApplyNBU = Input.ApplyNBU,
                ApplyBU = Input.ApplyBU,
                ApplyBVG = Input.ApplyBVG,
                ApplyFAK = Input.ApplyFAK,
                ApplyQST = Input.ApplyQST,

                PermitType = Input.PermitType,
                ChurchMember = Input.ChurchMember,
                Canton = Input.Canton,
                WithholdingTaxCode = Input.WithholdingTaxCode,

                Address = Input.Address,
                Zip = Input.Zip,
                City = Input.City,
                Phone = Input.Phone
            };

            var (success, _, message) =
                await _api.PutAsync<string>($"/api/Employee/{Input.Id}", body);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message ?? "Aktualisierung fehlgeschlagen.");
                return Page();
            }

            TempData["Alert"] = "Mitarbeiter erfolgreich aktualisiert.";
            return RedirectToPage("/Companies/Details", new { id = Input.CompanyId });
        }

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

        private async Task LoadQstTariffsAsync(string? canton)
        {
            var c = string.IsNullOrWhiteSpace(canton) ? "ZH" : canton.Trim().ToUpperInvariant();

            var (ok, data, msg) =
                await _api.GetAsync<List<QstTariffLookupDto>>($"/api/Lookups/qst-tariffs?canton={c}");

            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- bitte wählen --" }
            };

            if (ok && data is not null)
            {
                foreach (var t in data)
                {
                    list.Add(new SelectListItem
                    {
                        Value = t.Code,
                        Text = $"{t.Code} – {t.Description}"
                    });
                }
            }

            QstTariffCodes = list;
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
