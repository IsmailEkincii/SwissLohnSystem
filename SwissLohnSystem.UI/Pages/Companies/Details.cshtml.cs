using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Linq;
using SwissLohnSystem.UI.DTOs.Companies;   // CompanyDto
using SwissLohnSystem.UI.DTOs.Employees;  // EmployeeDto (full)
using SwissLohnSystem.UI.DTOs.Lohn;       // LohnDto  (UI'de yoksa ekle)
using SwissLohnSystem.UI.Responses;       // ApiResponse<T>

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public DetailsModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public CompanyDto? Company { get; private set; }
        public List<EmployeeDto> Employees { get; private set; } = new();
        public List<LohnDto> Lohns { get; private set; } = new();

        [TempData] public string? Toast { get; set; }
        [TempData] public string? Alert { get; set; }
        [TempData] public string? Error { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id <= 0)
            {
                TempData["Error"] = "Ungültige Firmen-ID.";
                return RedirectToPage("/Companies/Index");
            }

            var api = _httpClientFactory.CreateClient("ApiClient");

            // 1) Firma
            var companyResp = await api.GetFromJsonAsync<ApiResponse<CompanyDto>>($"/api/Company/{Id}");
            if (companyResp is null || !companyResp.Success || companyResp.Data is null)
            {
                TempData["Error"] = companyResp?.Message ?? "Firma wurde nicht gefunden.";
                return RedirectToPage("/Companies/Index");
            }
            Company = companyResp.Data;

            // 2) Mitarbeiter (firma bazlý)
            try
            {
                var empResp = await api.GetFromJsonAsync<ApiResponse<IEnumerable<EmployeeDto>>>($"/api/Company/{Id}/Employees");
                if (empResp?.Success == true && empResp.Data is not null)
                    Employees = empResp.Data.ToList();
            }
            catch
            {
                Employees = new();
            }

            // 3) Löhne (geçici: tüm Lohnlarý çek, çalýþan Id’lerine göre filtrele)
            try
            {
                // ÝLERÝDE: /api/Lohn/by-company/{Id} yazarsak doðrudan bu çaðrýyý kullanýrýz.
                var lohnResp = await api.GetFromJsonAsync<ApiResponse<IEnumerable<LohnDto>>>("/api/Lohn");
                if (lohnResp?.Success == true && lohnResp.Data is not null && Employees.Count > 0)
                {
                    var empIds = Employees.Select(e => e.Id).ToHashSet();
                    Lohns = lohnResp.Data
                        .Where(l => empIds.Contains(l.EmployeeId))
                        .OrderByDescending(l => l.Year).ThenByDescending(l => l.Month)
                        .ToList();
                }
            }
            catch
            {
                Lohns = new();
            }

            return Page();
        }
    }
}
