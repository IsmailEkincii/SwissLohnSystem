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

        [BindProperty]
        public List<SettingDto> Settings { get; set; } = new();

        [BindProperty]
        public List<QstTariffDto> QstTariffs { get; set; } = new();

        // Yeni eklenecek tarif
        [BindProperty]
        public QstTariffDto NewQst { get; set; } = new();

        // CSV file (Import)
        [BindProperty]
        public IFormFile? QstCsvFile { get; set; }

        public async Task OnGetAsync()
        {
            // SETTINGS
            var (ok, data, message) = await _api.GetAsync<List<SettingDto>>("/api/Settings");
            if (ok && data != null)
                Settings = data.OrderBy(s => s.Name).ToList();
            else
                TempData["Error"] = message ?? "Einstellungen konnten nicht geladen werden.";

            // QST
            var (ok2, qst, msg2) = await _api.GetAsync<List<QstTariffDto>>("/api/Settings/qst-tariffs");
            if (ok2 && qst != null)
            {
                QstTariffs = qst
                    .OrderBy(x => x.Canton)
                    .ThenBy(x => x.Code)
                    .ThenBy(x => x.PermitType)
                    .ThenBy(x => x.ChurchMember)
                    .ThenBy(x => x.IncomeFrom)
                    .ToList();
            }
            else
            {
                QstTariffs = new List<QstTariffDto>();
                if (!string.IsNullOrWhiteSpace(msg2))
                    TempData["Error"] = msg2;
            }

            // Yeni form her açýlýþta temiz olsun
            NewQst = new QstTariffDto
            {
                Canton = "ZH",
                Code = "A",
                PermitType = "B",
                ChurchMember = false,
                IncomeFrom = 0,
                IncomeTo = 0,
                Rate = 0,
                Remark = null
            };
        }

        // SETTINGS SAVE
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Bitte Eingaben prüfen.";
                return Page();
            }

            var (ok, _, message) = await _api.PutAsync<object>("/api/Settings", Settings);

            if (ok)
                TempData["Toast"] = "Einstellungen erfolgreich gespeichert.";
            else
                TempData["Error"] = message ?? "Speichern fehlgeschlagen.";

            return RedirectToPage();
        }

        // QST SAVE (mevcut tarifleri güncelle)
        public async Task<IActionResult> OnPostSaveQstAsync()
        {
            QstTariffs ??= new List<QstTariffDto>();

            // Sadece Id > 0 olanlarý update ediyoruz
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
                    TempData["Error"] =
                        $"Income From darf nicht größer als Income To sein ({t.Canton} {t.Code}).";
                    return RedirectToPage();
                }

                if (t.Rate < 0)
                {
                    TempData["Error"] =
                        $"Rate darf nicht negativ sein ({t.Canton} {t.Code}).";
                    return RedirectToPage();
                }
            }

            // Bulk upsert
            var (ok, _, message) =
                await _api.PutAsync<object>("/api/Settings/qst-tariffs", QstTariffs);

            if (ok)
                TempData["Toast"] = "QST-Tarife erfolgreich gespeichert.";
            else
                TempData["Error"] = message ?? "Speichern der QST-Tarife fehlgeschlagen.";

            return RedirectToPage();
        }

        // QST DELETE (tek satýr)
        public async Task<IActionResult> OnPostDeleteQstAsync(int id)
        {
            var (ok, _, message) =
                await _api.DeleteAsync<object>($"/api/Settings/qst-tariffs/{id}");

            if (ok)
                TempData["Toast"] = "QST-Tarif gelöscht.";
            else
                TempData["Error"] = message ?? "Löschen fehlgeschlagen.";

            return RedirectToPage();
        }

        // QST CREATE (yeni satýr ekle)
        public async Task<IActionResult> OnPostCreateQstAsync()
        {
            // Basit validasyon
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

            if (NewQst.Rate < 0)
            {
                TempData["Error"] = "Rate darf nicht negativ sein.";
                return RedirectToPage();
            }

            // Normalize
            NewQst.Canton = NewQst.Canton?.Trim().ToUpperInvariant();
            NewQst.Code = NewQst.Code?.Trim().ToUpperInvariant();
            NewQst.PermitType = NewQst.PermitType?.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(NewQst.Remark))
                NewQst.Remark = null;
            else
                NewQst.Remark = NewQst.Remark!.Trim();

            // API: POST /api/Settings/qst-tariffs  (tekil upsert)
            var (ok, created, message) =
                await _api.PostAsync<QstTariffDto>("/api/Settings/qst-tariffs", NewQst);

            if (ok && created != null)
                TempData["Toast"] = "QST-Tarif wurde erstellt.";
            else
                TempData["Error"] = message ?? "Erstellen des QST-Tarifs fehlgeschlagen.";

            return RedirectToPage();
        }

        // QST CSV IMPORT
        public async Task<IActionResult> OnPostImportQstCsvAsync()
        {
            if (QstCsvFile is null || QstCsvFile.Length == 0)
            {
                TempData["Error"] = "Bitte eine CSV-Datei auswählen.";
                return RedirectToPage();
            }

            var (ok, data, msg) = await _api.PostMultipartAsync<Dictionary<string, object>>(
                "/api/Settings/qst-tariffs/import",
                QstCsvFile,
                "file"
            );

            if (!ok)
            {
                TempData["Error"] = msg ?? "Import fehlgeschlagen.";
                return RedirectToPage();
            }

            var imported = data is not null && data.TryGetValue("imported", out var imp) ? imp?.ToString() : "?";
            var groups = data is not null && data.TryGetValue("groups", out var grp) ? grp?.ToString() : "?";

            TempData["Toast"] = $"Import OK: {imported} Zeilen, {groups} Gruppen aktualisiert.";
            return RedirectToPage();
        }
    }
}
