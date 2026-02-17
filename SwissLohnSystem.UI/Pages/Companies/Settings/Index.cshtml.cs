using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Qst;
using SwissLohnSystem.UI.DTOs.Setting;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Companies.Settings
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        public IndexModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)]
        public int CompanyId { get; set; }

        public string CompanyName { get; private set; } = "";

        // TAB1
        [BindProperty] public List<SettingDto> Settings { get; set; } = new();

        // TAB2
        [BindProperty] public List<QstTariffDto> QstTariffs { get; set; } = new();
        [BindProperty] public QstTariffDto NewQst { get; set; } = new();
        [BindProperty] public IFormFile? QstCsvFile { get; set; }

        // ✅ Rate raw string (virgül/dot destek)
        [BindProperty] public List<string> QstRateRaws { get; set; } = new(); // satır bazında
        [BindProperty] public string? NewQstRateRaw { get; set; }            // create form

        // TAB3 BVG
        public List<BvgPlanListItemDto> BvgPlans { get; private set; } = new();
        [BindProperty] public string? SelectedBvgPlanCode { get; set; }
        [BindProperty] public CreateOrUpdateBvgPlanDto BvgForm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int companyId, string? selectedBvgPlanCode = null)
        {
            CompanyId = companyId;
            SelectedBvgPlanCode = selectedBvgPlanCode;

            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");

            await LoadCompanyAsync();
            await LoadSettingsAsync();
            await LoadQstAsync();
            await LoadBvgPlansAsync();

            // default new qst
            NewQst = new QstTariffDto
            {
                Canton = "ZH",
                Code = "A0N",
                PermitType = "B",
                ChurchMember = false,
                IncomeFrom = 0,
                IncomeTo = 0,
                Rate = 0m,
                DisplayRate = 0m,
                Remark = null
            };
            NewQstRateRaw = "0";

            // BVG form
            if (!string.IsNullOrWhiteSpace(SelectedBvgPlanCode))
                await LoadBvgPlanDetailToFormAsync(SelectedBvgPlanCode!);
            else
                InitDefaultBvgForm();

            return Page();
        }

        // ======================
        // TAB1 SAVE SETTINGS
        // PUT /api/Settings?companyId=...
        // ======================
        public async Task<IActionResult> OnPostSaveSettingsAsync()
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");

            var payload = (Settings ?? new List<SettingDto>())
                .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                .Select(s => new SettingUpsertDto
                {
                    Name = s.Name.Trim(),
                    Value = (s.Value ?? "").Trim(),
                    Description = string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim()
                })
                .ToList();

            var (ok, resp, msg) = await _api.PutAsync<ApiResponse<object>>(
                $"/api/Settings?companyId={CompanyId}",
                payload);

            TempData[ok && resp?.Success == true ? "Toast" : "Error"] =
                resp?.Message ?? msg ?? (ok ? "Einstellungen gespeichert." : "Speichern fehlgeschlagen.");

            return RedirectToPage(new { companyId = CompanyId });
        }

        // ======================
        // TAB2 SAVE QST (UPDATE)
        // PUT /api/Settings/qst-tariffs?companyId=...
        // ======================
        public async Task<IActionResult> OnPostSaveQstAsync()
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");

            QstTariffs ??= new();
            QstRateRaws ??= new();

            // güvenli: rate raw listesi satır sayısına eşit olsun
            while (QstRateRaws.Count < QstTariffs.Count) QstRateRaws.Add("0");

            for (int i = 0; i < QstTariffs.Count; i++)
            {
                var t = QstTariffs[i];

                // boş satırları atla
                if (string.IsNullOrWhiteSpace(t.Canton) ||
                    string.IsNullOrWhiteSpace(t.Code) ||
                    string.IsNullOrWhiteSpace(t.PermitType))
                    continue;

                t.Canton = t.Canton.Trim().ToUpperInvariant();
                t.Code = t.Code.Trim().ToUpperInvariant();
                t.PermitType = t.PermitType.Trim().ToUpperInvariant();
                t.Remark = string.IsNullOrWhiteSpace(t.Remark) ? null : t.Remark.Trim();

                if (t.IncomeFrom < 0 || t.IncomeTo < t.IncomeFrom)
                {
                    TempData["Error"] = $"Ungültiger Einkommensbereich ({t.Canton} {t.Code}).";
                    return RedirectToPage(new { companyId = CompanyId });
                }

                var raw = QstRateRaws[i];
                var rate = ParseDecimalFlexible(raw);

                if (rate < 0)
                {
                    TempData["Error"] = $"Rate darf nicht negativ sein ({t.Canton} {t.Code}).";
                    return RedirectToPage(new { companyId = CompanyId });
                }

                // ✅ artık yüzde dönüştürme yok: ne girdiyse o
                t.Rate = rate;
            }

            // API PUT sadece id>0 olanları günceller
            var payload = QstTariffs.Where(x => x.Id > 0).ToList();

            var (ok, resp, msg) = await _api.PutAsync<ApiResponse<object>>(
                $"/api/Settings/qst-tariffs?companyId={CompanyId}",
                payload);

            TempData[ok && resp?.Success == true ? "Toast" : "Error"] =
                resp?.Message ?? msg ?? (ok ? "QST gespeichert." : "QST speichern fehlgeschlagen.");

            return RedirectToPage(new { companyId = CompanyId });
        }

        // ======================
        // TAB2 DELETE QST
        // DELETE /api/Settings/qst-tariffs/{id}?companyId=...
        // ======================
        public async Task<IActionResult> OnPostDeleteQstAsync(int id)
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");
            if (id <= 0)
            {
                TempData["Error"] = "Ungültige Id.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            var (ok, resp, msg) = await _api.DeleteAsync<ApiResponse<object>>(
                $"/api/Settings/qst-tariffs/{id}?companyId={CompanyId}");

            TempData[ok && resp?.Success == true ? "Toast" : "Error"] =
                resp?.Message ?? msg ?? (ok ? "QST-Tarif gelöscht." : "Löschen fehlgeschlagen.");

            return RedirectToPage(new { companyId = CompanyId });
        }

        // ======================
        // TAB2 CREATE QST
        // POST /api/Settings/qst-tariffs?companyId=...
        // ======================
        public async Task<IActionResult> OnPostCreateQstAsync()
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");

            if (string.IsNullOrWhiteSpace(NewQst.Canton) ||
                string.IsNullOrWhiteSpace(NewQst.Code) ||
                string.IsNullOrWhiteSpace(NewQst.PermitType))
            {
                TempData["Error"] = "Canton, Code und PermitType sind erforderlich.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            if (NewQst.IncomeFrom < 0 || NewQst.IncomeTo < NewQst.IncomeFrom)
            {
                TempData["Error"] = "Ungültiger Einkommensbereich.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            var rate = ParseDecimalFlexible(NewQstRateRaw);
            if (rate < 0)
            {
                TempData["Error"] = "Rate darf nicht negativ sein.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            NewQst.Canton = NewQst.Canton.Trim().ToUpperInvariant();
            NewQst.Code = NewQst.Code.Trim().ToUpperInvariant();
            NewQst.PermitType = NewQst.PermitType.Trim().ToUpperInvariant();
            NewQst.Remark = string.IsNullOrWhiteSpace(NewQst.Remark) ? null : NewQst.Remark.Trim();

            // ✅ artık yüzde dönüştürme yok
            NewQst.Rate = rate;

            var (ok, resp, msg) = await _api.PostAsync<ApiResponse<QstTariffDto>>(
                $"/api/Settings/qst-tariffs?companyId={CompanyId}",
                NewQst);

            TempData[ok && resp?.Success == true ? "Toast" : "Error"] =
                resp?.Message ?? msg ?? (ok ? "QST erstellt." : "Erstellen fehlgeschlagen.");

            return RedirectToPage(new { companyId = CompanyId });
        }

        // ======================
        // TAB2 CSV IMPORT
        // POST multipart /api/Settings/qst-tariffs/import?companyId=...
        // ======================
        public async Task<IActionResult> OnPostImportQstCsvAsync()
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");

            if (QstCsvFile is null || QstCsvFile.Length == 0)
            {
                TempData["Error"] = "Bitte eine CSV-Datei auswählen.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            // Eğer API "File" istiyorsa burada "File" yaz. (Sende hangisi çalışıyorsa o)
            const string formFieldName = "file";

            var (ok, resp, msg) = await _api.PostMultipartAsync<ApiResponse<object>>(
                $"/api/Settings/qst-tariffs/import?companyId={CompanyId}",
                QstCsvFile,
                formFieldName: formFieldName);

            TempData[ok && resp?.Success == true ? "Toast" : "Error"] =
                resp?.Message ?? msg ?? (ok ? "CSV importiert." : "Import fehlgeschlagen.");

            return RedirectToPage(new { companyId = CompanyId });
        }

        // ======================
        // TAB3 BVG SELECT
        // ======================
        public async Task<IActionResult> OnPostSelectBvgAsync(string planCode)
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");
            planCode = (planCode ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(planCode))
                return RedirectToPage(new { companyId = CompanyId });

            // Redirect ile hem tab state hem de detail loading garanti
            return Redirect($"/Companies/{CompanyId}/Settings?selectedBvgPlanCode={planCode}#tab-bvg");
        }

        // ======================
        // TAB3 BVG SAVE
        // POST /api/Settings/bvg-plans?companyId=...
        // ======================
        public async Task<IActionResult> OnPostSaveBvgAsync()
        {
            if (CompanyId <= 0) return RedirectToPage("/Companies/Index");

            BvgForm.PlanBaseCode = (BvgForm.PlanBaseCode ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(BvgForm.PlanBaseCode))
            {
                TempData["Error"] = "PlanBaseCode ist erforderlich.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            if (BvgForm.Year < 2000 || BvgForm.Year > 2100)
            {
                TempData["Error"] = "Ungültiges Jahr.";
                return RedirectToPage(new { companyId = CompanyId });
            }

            var (ok, resp, msg) = await _api.PostAsync<ApiResponse<object>>(
                $"/api/Settings/bvg-plans?companyId={CompanyId}",
                BvgForm);

            TempData[ok && resp?.Success == true ? "Toast" : "Error"] =
                resp?.Message ?? msg ?? (ok ? "BVG gespeichert." : "BVG speichern fehlgeschlagen.");

            return Redirect($"/Companies/{CompanyId}/Settings#tab-bvg");
        }

        // ======================
        // LOADERS
        // ======================
        private async Task LoadCompanyAsync()
        {
            var (ok, c, _) = await _api.GetAsync<CompanyDto>($"/api/Company/{CompanyId}");
            CompanyName = ok && c is not null ? c.Name : $"Firma #{CompanyId}";
        }

        private async Task LoadSettingsAsync()
        {
            var (ok, resp, msg) = await _api.GetAsync<ApiResponse<List<SettingDto>>>(
                $"/api/Settings?companyId={CompanyId}");

            if (!ok || resp?.Success != true || resp.Data is null)
            {
                Settings = new();
                if (!string.IsNullOrWhiteSpace(msg ?? resp?.Message))
                    TempData["Error"] = msg ?? resp?.Message;
                return;
            }

            Settings = resp.Data.OrderBy(s => s.Name).ToList();
        }

        private async Task LoadQstAsync()
        {
            var (ok, resp, msg) = await _api.GetAsync<ApiResponse<List<QstTariffDto>>>(
                $"/api/Settings/qst-tariffs?companyId={CompanyId}");

            if (!ok || resp?.Success != true || resp.Data is null)
            {
                QstTariffs = new();
                QstRateRaws = new();
                if (!string.IsNullOrWhiteSpace(msg ?? resp?.Message))
                    TempData["Error"] = msg ?? resp?.Message;
                return;
            }

            QstTariffs = resp.Data
                .OrderBy(x => x.Canton)
                .ThenBy(x => x.Code)
                .ThenBy(x => x.PermitType)
                .ThenBy(x => x.ChurchMember)
                .ThenBy(x => x.IncomeFrom)
                .ToList();

            // ✅ ekranda "so wie eingegeben" gibi göster: yüzde yok, dönüştürme yok
            // (db decimal -> string normalize)
            QstRateRaws = QstTariffs
                .Select(t => t.Rate.ToString("0.####", CultureInfo.InvariantCulture))
                .ToList();
        }

        private async Task LoadBvgPlansAsync()
        {
            var (ok, resp, msg) = await _api.GetAsync<ApiResponse<List<BvgPlanListItemDto>>>(
                $"/api/Settings/bvg-plans?companyId={CompanyId}");

            if (!ok || resp?.Success != true || resp.Data is null)
            {
                BvgPlans = new();
                if (!string.IsNullOrWhiteSpace(msg ?? resp?.Message))
                    TempData["Error"] = msg ?? resp?.Message;
                return;
            }

            BvgPlans = resp.Data
                .OrderByDescending(x => x.Year ?? 0)
                .ThenBy(x => x.Code)
                .ToList();
        }

        private async Task LoadBvgPlanDetailToFormAsync(string planCode)
        {
            planCode = (planCode ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(planCode))
            {
                InitDefaultBvgForm();
                return;
            }

            var (ok, resp, msg) = await _api.GetAsync<ApiResponse<BvgPlanDetailDto>>(
                $"/api/Settings/bvg-plans/{planCode}?companyId={CompanyId}");

            if (!ok || resp?.Success != true || resp.Data is null)
            {
                InitDefaultBvgForm();
                if (!string.IsNullOrWhiteSpace(msg ?? resp?.Message))
                    TempData["Error"] = msg ?? resp?.Message;
                return;
            }

            var d = resp.Data;
            SelectedBvgPlanCode = d.PlanCode;

            BvgForm = new CreateOrUpdateBvgPlanDto
            {
                PlanBaseCode = d.PlanBaseCode,
                Year = d.Year,
                DisplayName = null,
                CoordinationDedAnnual = d.CoordinationDedAnnual,
                EntryThresholdAnnual = d.EntryThresholdAnnual,
                UpperLimitAnnual = d.UpperLimitAnnual,
                Rate25_34_Employee = d.Rate25_34_Employee,
                Rate25_34_Employer = d.Rate25_34_Employer,
                Rate35_44_Employee = d.Rate35_44_Employee,
                Rate35_44_Employer = d.Rate35_44_Employer,
                Rate45_54_Employee = d.Rate45_54_Employee,
                Rate45_54_Employer = d.Rate45_54_Employer,
                Rate55_65_Employee = d.Rate55_65_Employee,
                Rate55_65_Employer = d.Rate55_65_Employer
            };
        }

        private void InitDefaultBvgForm()
        {
            BvgForm = new CreateOrUpdateBvgPlanDto
            {
                PlanBaseCode = "PK_STD",
                Year = DateTime.Now.Year,
                DisplayName = null,
                CoordinationDedAnnual = 0,
                EntryThresholdAnnual = 0,
                UpperLimitAnnual = 0
            };
        }

        // ======================
        // HELPERS
        // ======================
        private static decimal ParseDecimalFlexible(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0m;
            var s = raw.Trim();

            // de-CH: "0,14"
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("de-CH"), out var de))
                return de;

            // invariant: "0.14"
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var inv))
                return inv;

            // fallback: virgül -> nokta
            s = s.Replace(',', '.');
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var inv2))
                return inv2;

            return 0m;
        }

        // ======================
        // Local API response + SettingUpsertDto
        // (UI tarafı API contract’ına uyumlu olsun diye)
        // ======================
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }

        public class SettingUpsertDto
        {
            public string Name { get; set; } = "";
            public string? Value { get; set; }
            public string? Description { get; set; }
        }

        public class BvgPlanListItemDto
        {
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public int? Year { get; set; }
        }

        public class BvgPlanDetailDto
        {
            public int CompanyId { get; set; }
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
    }
}
