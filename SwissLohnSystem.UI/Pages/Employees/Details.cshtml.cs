using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _api;

        public string BaseUrl { get; }

        public EmployeeDto? Employee { get; private set; }
        public CompanyDto? Company { get; private set; }
        public List<LohnDto> LohnList { get; private set; } = new();
        public string? LoadError { get; private set; }
        public string DefaultPeriod { get; private set; } = $"{DateTime.Today:yyyy-MM}";

        public DetailsModel(ApiClient api)
        {
            _api = api;
            BaseUrl = _api.BaseUrl;
        }

        public async Task OnGetAsync(int id)
        {
            // JS için API base URL
            ViewData["ApiBaseUrl"] = BaseUrl?.TrimEnd('/');

            // Mitarbeiter laden
            var empRes = await _api.GetAsync<EmployeeDto>($"/api/Employee/{id}");
            if (!empRes.ok || empRes.data is null)
            {
                LoadError = empRes.message ?? "Mitarbeiter konnte nicht geladen werden.";
                return;
            }
            Employee = empRes.data;

            // Firma laden
            var compRes = await _api.GetAsync<CompanyDto>($"/api/Company/{Employee.CompanyId}");
            if (compRes.ok && compRes.data is not null)
                Company = compRes.data;

            // Lohnverlauf laden
            var byEmpRes = await _api.GetAsync<IEnumerable<LohnDto>>($"/api/Lohn/by-employee/{id}");
            if (byEmpRes.ok && byEmpRes.data is not null)
            {
                LohnList = byEmpRes.data
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .ToList();
            }
        }
    }
}
