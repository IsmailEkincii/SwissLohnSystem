using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Setting;
using SwissLohnSystem.UI.DTOs.Qst;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Settings
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        public IndexModel(ApiClient api) => _api = api;

        [BindProperty] public List<SettingDto> Settings { get; set; } = new();
        [BindProperty] public List<QstTariffDto> QstTariffs { get; set; } = new();

        [BindProperty] public QstTariffDto NewQst { get; set; } = new();
        [BindProperty] public IFormFile? QstCsvFile { get; set; }

        // ✅ BVG
        public List<BvgPlanListItemDto> BvgPlans { get; private set; } = new();

        [BindProperty] public string? SelectedBvgPlanCode { get; set; }

        [BindProperty] public CreateBvgPlanVm CreateBvg { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadSettingsAsync();
            await LoadQstAsync();
            await LoadBvgPlansAsync();

            NewQst = new QstTariffDto
            {
                Canton = "ZH",
                Code = "A",
                PermitType = "B",
                ChurchMember = false,
                IncomeFrom = 0,
                IncomeTo = 0,
                Rate = 0m,            // API/DB (0..1)
                DisplayRate = 0m,     // UI (%)
                Remark = null
            };

            CreateBvg = new CreateBvgPlanVm
            {
                PlanBaseCode = "PK_ZURICH_STD",
                Year = DateTime.Today.Year,

                CoordinationDedAnnual = 26460m,
                EntryThresholdAnnual = 22680m,
                UpperLimitAnnual = 90720m,

                Rate25_34_Employee = 0.07m,
                Rate25_34_Employer = 0.07m,

                Rate35_44_Employee = 0.10m,
                Rate35_44_Employer = 0.10m,

                Rate45_54_Employee = 0.15m,
                Rate45_54_Employer = 0.15m,

                Rate55_65_Employee = 0.18m,
                Rate55_65_Employer = 0.18m
            };
        }

        // =========================
        // SETTINGS SAVE
        // =========================
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Bitte Eingaben prüfen.";
                return Page();
            }

            var (ok, resp, message) = await _api.PutAsync<ApiResponse<object>>("/api/Settings", Settings);

            if (ok && resp?.Success == true)
                TempData["Toast"] = "Einstellungen erfolgreich gespeichert.";
            else
                TempData["Error"] = message ?? resp?.Message ?? "Speichern fehlgeschlagen.";

            return RedirectToPage();
        }

        // =========================
        // QST SAVE  (UI % -> API 0..1)
        // =========================
        public async Task<IActionResult> OnPostSaveQstAsync()
        {
            QstTariffs ??= new List<QstTariffDto>();

            QstTariffs = QstTariffs
                .Where(x =>
                    x.Id > 0 &&
                    !string.IsNullOrWhiteSpace(x.Canton) &&
                    !string.IsNullOrWhiteSpace(x.Code) &&
                    !string.IsNullOrWhiteSpace(x.PermitType))
                .ToList();

            foreach (var t in QstTariffs)
            {
                if (t.IncomeFrom > t.IncomeTo)
                {
                    TempData["Error"] = $"Income From darf nicht größer als Income To sein ({t.Canton} {t.Code}).";
                    return RedirectToPage();
                }

                // ✅ UI'da yüzde giriliyor (4.5 gibi)
                if (t.DisplayRate < 0)
                {
                    TempData["Error"] = $"Rate darf nicht negativ sein ({t.Canton} {t.Code}).";
                    return RedirectToPage();
                }

                // ✅ UI (%) -> API rate (0..1)
                t.Rate = t.DisplayRate / 100m;
            }

            var (ok, resp, message) = await _api.PutAsync<ApiResponse<object>>("/api/Settings/qst-tariffs", QstTariffs);

            if (ok && resp?.Success == true)
                TempData["Toast"] = "QST-Tarife erfolgreich gespeichert.";
            else
                TempData["Error"] = message ?? resp?.Message ?? "Speichern der QST-Tarife fehlgeschlagen.";

            return RedirectToPage();
        }

        // =========================
        // QST DELETE
        // =========================
        public async Task<IActionResult> OnPostDeleteQstAsync(int id)
        {
            var (ok, resp, message) = await _api.DeleteAsync<ApiResponse<object>>($"/api/Settings/qst-tariffs/{id}");

            if (ok && resp?.Success == true)
                TempData["Toast"] = "QST-Tarif gelöscht.";
            else
                TempData["Error"] = message ?? resp?.Message ?? "Löschen fehlgeschlagen.";

            return RedirectToPage();
        }

        // =========================
        // QST CREATE  (UI % -> API 0..1)
        // =========================
        public async Task<IActionResult> OnPostCreateQstAsync()
        {
            if (string.IsNullOrWhiteSpace(NewQst.Canton) ||
                string.IsNullOrWhiteSpace(NewQst.Code) ||
                string.IsNullOrWhiteSpace(NewQst.PermitType))
            {
                TempData["Error"] = "Kanton, Code und Bewilligung sind erforderlich.";
                return RedirectToPage();
            }

            if (NewQst.IncomeFrom < 0 || NewQst.IncomeTo < NewQst.IncomeFrom)
            {
                TempData["Error"] = "Income-Bereich ist ungültig.";
                return RedirectToPage();
            }

            // ✅ UI'da yüzde giriliyor
            if (NewQst.DisplayRate < 0)
            {
                TempData["Error"] = "Rate darf nicht negativ sein.";
                return RedirectToPage();
            }

            NewQst.Canton = NewQst.Canton.Trim().ToUpperInvariant();
            NewQst.Code = NewQst.Code.Trim().ToUpperInvariant();
            NewQst.PermitType = NewQst.PermitType.Trim().ToUpperInvariant();
            NewQst.Remark = string.IsNullOrWhiteSpace(NewQst.Remark) ? null : NewQst.Remark.Trim();

            // ✅ UI (%) -> API rate (0..1)
            NewQst.Rate = NewQst.DisplayRate / 100m;

            var (ok, resp, message) = await _api.PostAsync<ApiResponse<QstTariffDto>>("/api/Settings/qst-tariffs", NewQst);

            if (ok && resp?.Success == true)
                TempData["Toast"] = "QST-Tarif wurde erstellt.";
            else
                TempData["Error"] = message ?? resp?.Message ?? "Erstellen des QST-Tarifs fehlgeschlagen.";

            return RedirectToPage();
        }

        // =========================
        // QST CSV IMPORT
        // =========================
        public async Task<IActionResult> OnPostImportQstCsvAsync()
        {
            if (QstCsvFile is null || QstCsvFile.Length == 0)
            {
                TempData["Error"] = "Bitte eine CSV-Datei auswählen.";
                return RedirectToPage();
            }

            var (ok, resp, msg) = await _api.PostMultipartAsync<ApiResponse<Dictionary<string, object>>>(
                "/api/Settings/qst-tariffs/import",
                QstCsvFile,
                "file"
            );

            if (!ok || resp?.Success != true)
            {
                TempData["Error"] = msg ?? resp?.Message ?? "Import fehlgeschlagen.";
                return RedirectToPage();
            }

            var data = resp.Data;
            var imported = data is not null && data.TryGetValue("imported", out var imp) ? imp?.ToString() : "?";
            var groups = data is not null && data.TryGetValue("groups", out var grp) ? grp?.ToString() : "?";

            TempData["Toast"] = $"Import OK: {imported} Zeilen, {groups} Gruppen aktualisiert.";
            return RedirectToPage();
        }

        // =========================
        // ✅ BVG LOAD DETAIL
        // =========================
        public async Task<IActionResult> OnPostLoadBvgDetailAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedBvgPlanCode))
            {
                TempData["Error"] = "Bitte einen BVG-Plan auswählen.";
                return RedirectToPage();
            }

            await LoadSettingsAsync();
            await LoadQstAsync();
            await LoadBvgPlansAsync();

            var code = SelectedBvgPlanCode.Trim().ToUpperInvariant();

            var (ok, resp, msg) = await _api.GetAsync<ApiResponse<BvgPlanDetailDto>>($"/api/Settings/bvg-plans/{code}");

            if (!ok || resp?.Success != true || resp.Data == null)
            {
                TempData["Error"] = msg ?? resp?.Message ?? "BVG Plan konnte nicht geladen werden.";
                return Page();
            }

            var d = resp.Data;

            SelectedBvgPlanCode = d.PlanCode;

            CreateBvg.PlanBaseCode = d.PlanBaseCode;
            CreateBvg.Year = d.Year;

            CreateBvg.CoordinationDedAnnual = d.CoordinationDedAnnual;
            CreateBvg.EntryThresholdAnnual = d.EntryThresholdAnnual;
            CreateBvg.UpperLimitAnnual = d.UpperLimitAnnual;

            CreateBvg.Rate25_34_Employee = d.Rate25_34_Employee;
            CreateBvg.Rate25_34_Employer = d.Rate25_34_Employer;

            CreateBvg.Rate35_44_Employee = d.Rate35_44_Employee;
            CreateBvg.Rate35_44_Employer = d.Rate35_44_Employer;

            CreateBvg.Rate45_54_Employee = d.Rate45_54_Employee;
            CreateBvg.Rate45_54_Employer = d.Rate45_54_Employer;

            CreateBvg.Rate55_65_Employee = d.Rate55_65_Employee;
            CreateBvg.Rate55_65_Employer = d.Rate55_65_Employer;

            TempData["Toast"] = $"BVG-Plan geladen: {d.PlanCode}";
            return Page();
        }

        // =========================
        // ✅ BVG CREATE/UPDATE
        // =========================
        public async Task<IActionResult> OnPostCreateBvgPlanAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Bitte BVG Eingaben prüfen.";
                return RedirectToPage();
            }

            var body = new
            {
                planBaseCode = (CreateBvg.PlanBaseCode ?? "").Trim(),
                year = CreateBvg.Year,

                coordinationDedAnnual = CreateBvg.CoordinationDedAnnual,
                entryThresholdAnnual = CreateBvg.EntryThresholdAnnual,
                upperLimitAnnual = CreateBvg.UpperLimitAnnual,

                rate25_34_Employee = CreateBvg.Rate25_34_Employee,
                rate25_34_Employer = CreateBvg.Rate25_34_Employer,

                rate35_44_Employee = CreateBvg.Rate35_44_Employee,
                rate35_44_Employer = CreateBvg.Rate35_44_Employer,

                rate45_54_Employee = CreateBvg.Rate45_54_Employee,
                rate45_54_Employer = CreateBvg.Rate45_54_Employer,

                rate55_65_Employee = CreateBvg.Rate55_65_Employee,
                rate55_65_Employer = CreateBvg.Rate55_65_Employer
            };

            var (ok, resp, msg) = await _api.PostAsync<ApiResponse<Dictionary<string, object>>>("/api/Settings/bvg-plans", body);

            if (!ok || resp?.Success != true)
            {
                TempData["Error"] = msg ?? resp?.Message ?? "BVG-Plan konnte nicht gespeichert werden.";
                return RedirectToPage();
            }

            var planCode = resp.Data != null && resp.Data.TryGetValue("planCode", out var pc) ? pc?.ToString() : null;

            TempData["Toast"] = string.IsNullOrWhiteSpace(planCode)
                ? "BVG-Plan gespeichert."
                : $"BVG-Plan gespeichert: {planCode}";

            return RedirectToPage();
        }

        // =========================
        // LOADERS
        // =========================
        private async Task LoadSettingsAsync()
        {
            var (ok, resp, message) = await _api.GetAsync<ApiResponse<List<SettingDto>>>("/api/Settings");

            if (ok && resp?.Success == true && resp.Data != null)
                Settings = resp.Data.OrderBy(s => s.Name).ToList();
            else
                TempData["Error"] = message ?? resp?.Message ?? "Einstellungen konnten nicht geladen werden.";
        }

        private async Task LoadQstAsync()
        {
            var (ok, resp, message) = await _api.GetAsync<ApiResponse<List<QstTariffDto>>>("/api/Settings/qst-tariffs");

            if (ok && resp?.Success == true && resp.Data != null)
            {
                QstTariffs = resp.Data
                    .OrderBy(x => x.Canton)
                    .ThenBy(x => x.Code)
                    .ThenBy(x => x.PermitType)
                    .ThenBy(x => x.ChurchMember)
                    .ThenBy(x => x.IncomeFrom)
                    .ToList();

                // ✅ API rate (0..1) -> UI yüzde
                foreach (var t in QstTariffs)
                    t.DisplayRate = t.Rate * 100m;
            }
            else
            {
                QstTariffs = new List<QstTariffDto>();
                var err = message ?? resp?.Message;
                if (!string.IsNullOrWhiteSpace(err))
                    TempData["Error"] = err;
            }
        }

        private async Task LoadBvgPlansAsync()
        {
            var (ok, resp, msg) = await _api.GetAsync<ApiResponse<List<BvgPlanListItemDto>>>("/api/Settings/bvg-plans");

            if (ok && resp?.Success == true && resp.Data != null)
            {
                BvgPlans = resp.Data
                    .OrderByDescending(x => x.Year ?? 0)
                    .ThenBy(x => x.Code)
                    .ToList();
            }
            else
            {
                BvgPlans = new List<BvgPlanListItemDto>();
                var err = msg ?? resp?.Message;
                if (!string.IsNullOrWhiteSpace(err))
                    TempData["Error"] = err;
            }
        }

        // =========================
        // Small helper DTOs
        // =========================
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }

        public class BvgPlanListItemDto
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public int? Year { get; set; }
        }

        public class BvgPlanDetailDto
        {
            public string PlanCode { get; set; } = "";
            public string PlanBaseCode { get; set; } = "";
            public int Year { get; set; }

            public decimal CoordinationDedAnnual { get; set; }
            public decimal EntryThresholdAnnual { get; set; }
            public decimal UpperLimitAnnual { get; set; }

            public decimal Rate25_34_Employee { get; set; }
            public decimal Rate25_34_Employer { get; set; }

            public decimal Rate35_44_Employee { get; set; }
            public decimal Rate35_44_Employer { get; set; }

            public decimal Rate45_54_Employee { get; set; }
            public decimal Rate45_54_Employer { get; set; }

            public decimal Rate55_65_Employee { get; set; }
            public decimal Rate55_65_Employer { get; set; }
        }

        public class CreateBvgPlanVm
        {
            [Required(ErrorMessage = "PlanBaseCode ist erforderlich.")]
            [StringLength(60)]
            public string? PlanBaseCode { get; set; }

            [Range(2000, 2100, ErrorMessage = "Jahr ist ungültig.")]
            public int Year { get; set; }

            [Range(0, double.MaxValue)]
            public decimal CoordinationDedAnnual { get; set; }

            [Range(0, double.MaxValue)]
            public decimal EntryThresholdAnnual { get; set; }

            [Range(0, double.MaxValue)]
            public decimal UpperLimitAnnual { get; set; }

            // ✅ 8 alan (AN/AG)
            [Range(0, 100)] public decimal Rate25_34_Employee { get; set; }
            [Range(0, 100)] public decimal Rate25_34_Employer { get; set; }

            [Range(0, 100)] public decimal Rate35_44_Employee { get; set; }
            [Range(0, 100)] public decimal Rate35_44_Employer { get; set; }

            [Range(0, 100)] public decimal Rate45_54_Employee { get; set; }
            [Range(0, 100)] public decimal Rate45_54_Employer { get; set; }

            [Range(0, 100)] public decimal Rate55_65_Employee { get; set; }
            [Range(0, 100)] public decimal Rate55_65_Employer { get; set; }
        }
    }
}
