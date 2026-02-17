using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;
using SwissLohnSystem.UI.DTOs.Setting;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

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

        /// <summary>Lohn + Employee + Company birleşmiş detay DTO’su</summary>
        public LohnDetailsDto? Lohn { get; private set; }
        public CompanyDto? Company { get; private set; }
        public EmployeeDto? Employee { get; private set; }

        public string? Error { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            // JS için BaseUrl
            ViewData["ApiBaseUrl"] = _api.BaseUrl?.TrimEnd('/');

            // 1) Lohn kaydını çek
            var lohnRes = await _api.GetAsync<LohnDto>($"/api/Lohn/{id}");
            if (!lohnRes.ok || lohnRes.data is null)
            {
                Error = lohnRes.message ?? "Lohnabrechnung wurde nicht gefunden.";
                Lohn = null;
                return Page();
            }

            var lohn = lohnRes.data;

            // 2) Mitarbeiter detayları
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

            // ? 4) Settings oranlarını çek (Name -> Value)
            var rateByName = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // ❗ Mutlaka companyId ile çağır
            var settingsRes = await _api.GetAsync<List<SettingDto>>($"/api/Settings?companyId={comp?.Id ?? 0}");

            if (settingsRes.ok && settingsRes.data is not null)
            {
                foreach (var s in settingsRes.data)
                {
                    if (!string.IsNullOrWhiteSpace(s.Name))
                        rateByName[s.Name] = ParseDecimalFlexible(s.Value);
                }
            }


            // ? 5) UI DTO’su (Rate destekli overload)
            Lohn = ApiToUiMapper.ToDetails(lohn, emp, comp, rateByName);

            return Page();
        }
        private static decimal ParseDecimalFlexible(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0m;

            var s = raw.Trim();

            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var a)) return a;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("de-CH"), out var b)) return b;

            s = s.Replace(',', '.');
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var c)) return c;

            return 0m;
        }

    }
}
