using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class IndexModel : PageModel
    {
        private readonly ApiClient _api;
        public IndexModel(ApiClient api) => _api = api;

        public IEnumerable<CompanyDto> Companies { get; private set; } = Array.Empty<CompanyDto>();

        public async Task OnGetAsync()
        {
            var listRes = await _api.GetAsync<IEnumerable<CompanyDto>>("/api/Company");
            if (!listRes.ok || listRes.data is null)
            {
                TempData["Error"] = listRes.message ?? "Firmenliste konnte nicht geladen werden.";
                Companies = Array.Empty<CompanyDto>();
                return;
            }

            Companies = listRes.data;
            if (!string.IsNullOrWhiteSpace(listRes.message))
                TempData["Alert"] = listRes.message; // Ýstersen kapat
        }

        // POST: Delete handler
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Ungültige ID.";
                return RedirectToPage();
            }

            var delRes = await _api.DeleteAsync<string>($"/api/Company/{id}");
            if (!delRes.ok)
            {
                TempData["Error"] = delRes.message ?? "Löschen fehlgeschlagen.";
                return RedirectToPage();
            }

            TempData["Toast"] = delRes.message ?? "Firma wurde erfolgreich gelöscht.";
            return RedirectToPage();
        }
    }
}
