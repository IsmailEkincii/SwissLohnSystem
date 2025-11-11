using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using SwissLohnSystem.UI.DTOs.Companies;   // CompanyDto, CompanyUpdateDto
using SwissLohnSystem.UI.Responses;        // ApiResponse<T>

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public EditModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [BindProperty] public CompanyDto Input { get; set; } = new();

        [TempData] public string? Toast { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            var api = _httpClientFactory.CreateClient("ApiClient");

            // GET /api/Company/{id} -> ApiResponse<CompanyDto>
            var resp = await api.GetFromJsonAsync<ApiResponse<CompanyDto>>($"/api/Company/{id}");
            if (resp is null || !resp.Success || resp.Data is null)
            {
                TempData["Toast"] = resp?.Message ?? "Firma wurde nicht gefunden.";
                return RedirectToPage("/Companies/Index");
            }

            Input = resp.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var api = _httpClientFactory.CreateClient("ApiClient");

            var body = new CompanyUpdateDto
            {
                Id = Input.Id,
                Name = Input.Name.Trim(),
                Canton = Input.Canton,
                Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone.Trim(),
                TaxNumber = string.IsNullOrWhiteSpace(Input.TaxNumber) ? null : Input.TaxNumber.Trim()
            };

            // PUT /api/Company/{id} -> ApiResponse<string>
            var res = await api.PutAsJsonAsync($"/api/Company/{Input.Id}", body);

            ApiResponse<string>? payload = null;
            string? raw = null;
            try { payload = await res.Content.ReadFromJsonAsync<ApiResponse<string>>(); }
            catch { raw = await res.Content.ReadAsStringAsync(); }

            if (!res.IsSuccessStatusCode || payload is null || !payload.Success)
            {
                ModelState.AddModelError(string.Empty, payload?.Message ?? raw ?? "Aktualisierung fehlgeschlagen.");
                return Page();
            }

            TempData["Toast"] = payload.Message ?? "Firma wurde erfolgreich aktualisiert.";
            return RedirectToPage("/Companies/Index");
        }
    }
}
