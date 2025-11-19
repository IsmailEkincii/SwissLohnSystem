using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Mapping;

namespace SwissLohnSystem.UI.Pages.Lohn
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _api;

        public DetailsModel(ApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        /// <summary>Lohn + Employee + Company birleþmiþ detay DTO’su</summary>
        public LohnDetailsDto? Lohn { get; private set; }

        public string? Error { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            // JS için BaseUrl (ileride slip hesaplama istersek kullanýrýz)
            ViewData["ApiBaseUrl"] = _api.BaseUrl?.TrimEnd('/');

            // 1) Lohn kaydýný çek
            var lohnRes = await _api.GetAsync<LohnDto>($"/api/Lohn/{id}");
            if (!lohnRes.ok || lohnRes.data is null)
            {
                Error = lohnRes.message ?? "Lohnabrechnung wurde nicht gefunden.";
                return Page();
            }

            var lohn = lohnRes.data;

            // 2) Mitarbeiter detaylarý
            EmployeeDto? emp = null;
            CompanyDto? comp = null;

            var empRes = await _api.GetAsync<EmployeeDto>($"/api/Employee/{lohn.EmployeeId}");
            if (empRes.ok && empRes.data is not null)
            {
                emp = empRes.data;

                // 3) Firma
                var compRes = await _api.GetAsync<CompanyDto>($"/api/Company/{emp.CompanyId}");
                if (compRes.ok && compRes.data is not null)
                {
                    comp = compRes.data;
                }
            }

            // 4) UI DTO’su (ApiToUiMapper kullanýyoruz)
            Lohn = ApiToUiMapper.ToDetails(lohn, emp, comp);

            return Page();
        }
    }
}
