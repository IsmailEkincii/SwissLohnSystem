using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Setting;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Settings
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        public IndexModel(ApiClient api) => _api = api;

        [BindProperty]
        public List<SettingDto> Settings { get; set; } = new();

        public async Task OnGetAsync()
        {
            var (ok, data, message) = await _api.GetAsync<List<SettingDto>>("/api/Settings");
            if (ok && data != null) Settings = data.OrderBy(s => s.Name).ToList();
            else
            {
                TempData["Error"] = message ?? "Einstellungen konnten nicht geladen werden.";
            }
        }

       
        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Bitte Eingaben prüfen.";
                return Page();
            }

            var (ok, _, message) = await _api.PutAsync<object>("/api/Settings", Settings);
            if (ok) TempData["Toast"] = "Einstellungen erfolgreich gespeichert.";
            else TempData["Error"] = message ?? "Speichern fehlgeschlagen.";

            return RedirectToPage();
        }
    }
}
