using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;

namespace SwissLohnSystem.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Dashboard kutularý
        public int FirmaCount { get; set; }
        public int MitarbeiterCount { get; set; }
        public int LohnCountThisMonth { get; set; }
        public int OpenTasks { get; set; } = 0; // þimdilik sabit

        public async Task OnGet()
        {
            var api = _httpClientFactory.CreateClient("ApiClient");

            // 1) Firmen
            try
            {
                var firmen = await api.GetFromJsonAsync<List<CompanyDto>>("/api/Company");
                FirmaCount = firmen?.Count ?? 0;
            }
            catch { FirmaCount = 0; }

            // 2) Mitarbeiter
            try
            {
                var employees = await api.GetFromJsonAsync<List<EmployeeDto>>("/api/Employee");
                MitarbeiterCount = employees?.Count ?? 0;
            }
            catch { MitarbeiterCount = 0; }

            // 3) Lohn (ay filtresi yoksa tüm kayýtlarý sayar; istersen API’ye /api/Lohn?month=&year= ekleriz)
            try
            {
                var loehne = await api.GetFromJsonAsync<List<LohnDto>>("/api/Lohn");
                var now = DateTime.UtcNow; // istersen TimeZone ekle
                LohnCountThisMonth = loehne?
                    .Count(x => x.Month == now.Month && x.Year == now.Year) ?? 0;
            }
            catch { LohnCountThisMonth = 0; }

            // 4) OpenTasks: ileride Calendar entegrasyonunda baðlayacaðýz
        }

        // Basit DTO’lar (UI tarafý için)
        public record CompanyDto(int Id, string Name, string? Address, string Canton);
        public record EmployeeDto(int Id, int CompanyId, string FirstName, string LastName);
        public record LohnDto(int Id, int EmployeeId, int Month, int Year);
    }
}
