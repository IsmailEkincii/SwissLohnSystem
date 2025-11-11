using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public IndexModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public List<CompanyDto> Companies { get; set; } = new();

        public async Task OnGet() => await LoadAsync();

        private async Task LoadAsync()
        {
            var api = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                // API: ApiResponse<List<CompanyDto>> döner
                var resp = await api.GetFromJsonAsync<ApiResponse<List<CompanyDto>>>("/api/Company");
                Companies = (resp?.Success == true && resp.Data is not null) ? resp.Data : new();
            }
            catch
            {
                Companies = new();
            }
        }

        // ? Delete Firma (PRG pattern)
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Ungültige Firmen-ID.";
                return RedirectToPage();
            }

            var api = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var res = await api.DeleteAsync($"/api/Company/{id}");

                ApiResponse<string>? payload = null;
                string? raw = null;

                try { payload = await res.Content.ReadFromJsonAsync<ApiResponse<string>>(); }
                catch { raw = await res.Content.ReadAsStringAsync(); }

                if (res.IsSuccessStatusCode)
                {
                    TempData["Alert"] = payload?.Message ?? "Firma wurde erfolgreich gelöscht.";
                }
                else if (res.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = payload?.Message ?? "Firma wurde nicht gefunden.";
                }
                else
                {
                    var msg = payload?.Message ?? raw ?? $"HTTP {(int)res.StatusCode}";
                    TempData["Error"] = $"Löschen fehlgeschlagen: {msg}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Fehler: {ex.Message}";
            }

            return RedirectToPage(); // PRG: GET ile yeniden yükle
        }

        // ---------- DTOs & ApiResponse (UI Models) ----------
        public record CompanyDto(int Id, string Name, string? Address, string Canton);

        public class ApiResponse<T>
        {
            [JsonPropertyName("success")] public bool Success { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
