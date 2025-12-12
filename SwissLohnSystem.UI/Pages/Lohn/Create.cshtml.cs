using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;
using SwissLohnSystem.UI.DTOs.Payroll;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Lookups;

namespace SwissLohnSystem.UI.Pages.Lohn
{
    public class CreateModel : PageModel
    {
        private readonly ApiClient _api;
        public CreateModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)]
        public int EmployeeId { get; set; }

        public EmployeeDto? Employee { get; private set; }
        public CompanyDto? Company { get; private set; }

        // Dropdownlar
        public List<SelectListItem> PermitTypes { get; private set; } = new();
        public List<SelectListItem> QstTariffCodes { get; private set; } = new();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData] public string? Error { get; set; }
        [TempData] public string? Alert { get; set; }

        // ================================
        // GET /Lohn/Create?employeeId=..&period=YYYY-MM
        // ================================
        public async Task<IActionResult> OnGetAsync(int employeeId, string? period)
        {
            // JS tarafı için API base URL'yi ViewData'ya veriyoruz
            ViewData["ApiBaseUrl"] = _api.BaseUrl?.TrimEnd('/');

            EmployeeId = employeeId;

            if (employeeId <= 0)
            {
                Error = "Ungültige Mitarbeiter-ID.";
                return Page();
            }

            // 1) Mitarbeiter laden
            var (okEmp, emp, empMsg) =
                await _api.GetAsync<EmployeeDto>($"/api/Employee/{employeeId}");

            if (!okEmp || emp is null)
            {
                Error = empMsg ?? "Mitarbeiterdaten konnten nicht geladen werden.";
                return Page();
            }

            Employee = emp;

            // 2) Firma laden
            var (okCo, comp, coMsg) =
                await _api.GetAsync<CompanyDto>($"/api/Company/{emp.CompanyId}");
            if (okCo && comp is not null)
            {
                Company = comp;
            }

            // 4) Period (varsayılan bu ayın 1’i)
            DateTime periodDate;
            if (!string.IsNullOrWhiteSpace(period) && DateTime.TryParse(period, out var parsed))
            {
                periodDate = parsed;
            }
            else
            {
                periodDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            // 5) Input defaultları (Employee’den)
            Input.EmployeeId = emp.Id;
            Input.Period = periodDate;
            Input.GrossMonthly = emp.BruttoSalary;
            Input.WeeklyHours = emp.WeeklyHours;

            Input.Canton = string.IsNullOrWhiteSpace(emp.Canton)
                ? "ZH"
                : emp.Canton.Trim().ToUpperInvariant();

            Input.PermitType = string.IsNullOrWhiteSpace(emp.PermitType)
                ? "B"
                : emp.PermitType.Trim().ToUpperInvariant();

            Input.WithholdingTaxCode = string.IsNullOrWhiteSpace(emp.WithholdingTaxCode)
                ? null
                : emp.WithholdingTaxCode.Trim().ToUpperInvariant();

            Input.ApplyAHV = emp.ApplyAHV;
            Input.ApplyALV = emp.ApplyALV;
            Input.ApplyBVG = emp.ApplyBVG;
            Input.ApplyNBU = emp.ApplyNBU;
            Input.ApplyBU = emp.ApplyBU;
            Input.ApplyFAK = emp.ApplyFAK;
            Input.ApplyQST = emp.ApplyQST;
            Input.ChurchMember = emp.ChurchMember;

            // Gün / izin alanları
            Input.WorkedDays = null;
            Input.SickDays = null;
            Input.UnpaidDays = null;

            // Bonus / Zulagen default
            Input.Bonus = 0;
            Input.ExtraAllowance = 0;
            Input.UnpaidDeduction = 0;
            Input.OtherDeduction = 0;

            // 3) Dropdownları doldur (employee Kanton'una göre)
            await LoadLookupsAsync(Input.Canton);

            return Page();
        }

        // ================================
        // POST
        // ================================
        public async Task<IActionResult> OnPostAsync()
        {
            // JS için yine BaseUrl
            ViewData["ApiBaseUrl"] = _api.BaseUrl?.TrimEnd('/');

            // Employee & Company'yi POST'ta da yeniden yükleyelim
            if (Input.EmployeeId > 0)
            {
                var (okEmp, emp, empMsg) =
                    await _api.GetAsync<EmployeeDto>($"/api/Employee/{Input.EmployeeId}");

                if (okEmp && emp is not null)
                {
                    Employee = emp;

                    var (okCo, comp, _) =
                        await _api.GetAsync<CompanyDto>($"/api/Company/{emp.CompanyId}");
                    if (okCo && comp is not null)
                        Company = comp;
                }
                else
                {
                    Error = empMsg ?? "Mitarbeiterdaten konden nicht geladen werden.";
                }
            }

            // ---- Normalizasyon ----
            Input.Canton = string.IsNullOrWhiteSpace(Input.Canton)
                ? "ZH"
                : Input.Canton.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(Input.PermitType))
                Input.PermitType = "B";
            else
                Input.PermitType = Input.PermitType.Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(Input.WithholdingTaxCode))
                Input.WithholdingTaxCode = Input.WithholdingTaxCode.Trim().ToUpperInvariant();

            // Dropdownları (normalize sonrası) doldur
            await LoadLookupsAsync(Input.Canton);

            // ---- QST basit validasyon ----
            if (Input.ApplyQST)
            {
                if (string.IsNullOrWhiteSpace(Input.WithholdingTaxCode))
                {
                    ModelState.AddModelError(nameof(Input.WithholdingTaxCode),
                        "Bei Quellensteuer ist ein QST-Code erforderlich (z.B. A0).");
                }

                if (string.IsNullOrWhiteSpace(Input.PermitType))
                {
                    ModelState.AddModelError(nameof(Input.PermitType),
                        "Bei Quellensteuer ist eine Bewilligung erforderlich.");
                }
            }

            if (!ModelState.IsValid)
                return Page();

            // 🔥 DateTime -> DateOnly dönüşümü
            var dto = new PayrollRequestDto
            {
                EmployeeId = Input.EmployeeId,
                Period = DateOnly.FromDateTime(Input.Period),

                GrossMonthly = Input.GrossMonthly,
                Bonus = Input.Bonus,
                ExtraAllowance = Input.ExtraAllowance,
                UnpaidDeduction = Input.UnpaidDeduction,
                OtherDeduction = Input.OtherDeduction,

                ApplyAHV = Input.ApplyAHV,
                ApplyALV = Input.ApplyALV,
                ApplyBVG = Input.ApplyBVG,
                ApplyNBU = Input.ApplyNBU,
                ApplyBU = Input.ApplyBU,
                ApplyFAK = Input.ApplyFAK,
                ApplyQST = Input.ApplyQST,

                WeeklyHours = Input.WeeklyHours,
                Canton = Input.Canton,
                WithholdingTaxCode = Input.WithholdingTaxCode,
                PermitType = Input.PermitType,
                ChurchMember = Input.ChurchMember,

                WorkedDays = Input.WorkedDays,
                SickDays = Input.SickDays,
                UnpaidDays = Input.UnpaidDays,

                BvgPlan = null
            };

            var (ok, lohn, msg) =
                await _api.PostAsync<LohnDto>("/api/Lohn/calc-and-save", dto);

            if (!ok || lohn is null)
            {
                ModelState.AddModelError(string.Empty, msg ?? "Berechnung fehlgeschlagen.");
                return Page();
            }

            Alert = "Lohnabrechnung wurde erstellt.";
            return RedirectToPage("/Lohn/Details", new { id = lohn.Id });
        }

        // ================================
        // Lookup helper
        // ================================
        private async Task LoadLookupsAsync(string? canton)
        {
            // Bewilligunglar hala statik (UI lookups)
            PermitTypes = QstUiLookups.GetPermitTypes();

            var c = string.IsNullOrWhiteSpace(canton)
                ? "ZH"
                : canton.Trim().ToUpperInvariant();

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

        // ================================
        // Form ViewModel (aynı kaldı)
        // ================================
        public class InputModel
        {
            [Required]
            public int EmployeeId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime Period { get; set; }

            [Range(0, 1_000_000)]
            public decimal GrossMonthly { get; set; }

            public decimal Bonus { get; set; }
            public decimal ExtraAllowance { get; set; }
            public decimal UnpaidDeduction { get; set; }
            public decimal OtherDeduction { get; set; }

            [Range(0, 31)]
            public decimal? WorkedDays { get; set; }

            [Range(0, 31)]
            public decimal? SickDays { get; set; }

            [Range(0, 31)]
            public decimal? UnpaidDays { get; set; }

            public bool ApplyAHV { get; set; }
            public bool ApplyALV { get; set; }
            public bool ApplyBVG { get; set; }
            public bool ApplyNBU { get; set; }
            public bool ApplyBU { get; set; }
            public bool ApplyFAK { get; set; }
            public bool ApplyQST { get; set; }

            [Range(1, 60)]
            public int WeeklyHours { get; set; }

            [Required, StringLength(2)]
            public string Canton { get; set; } = "ZH";

            public string? WithholdingTaxCode { get; set; }

            [Required]
            public string PermitType { get; set; } = "B";

            public bool ChurchMember { get; set; }
        }
    }
}
