using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Mapping;

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _api;

        public DetailsModel(ApiClient api)
        {
            _api = api;
        }

        public int Id { get; set; }

        // View'da kullandığımız ViewModel
        public CompanyDetailsDto Vm { get; set; } = new();

        // API base adresi (appsettings / ApiClient'tan gelir)
        public string ApiBaseUrl => _api.BaseUrl?.TrimEnd('/') ?? string.Empty;

        public async Task OnGetAsync(int id)
        {
            Id = id;

            // JS tarafı için base URL
            ViewData["ApiBaseUrl"] = ApiBaseUrl;

            // Firma
            var (okCompany, company, companyMsg) =
                await _api.GetAsync<CompanyDto>($"/api/Company/{id}");

            // Firma bulunamazsa
            if (!okCompany || company is null)
            {
                ViewData["Error"] = companyMsg ?? "Firma konnte nicht geladen werden.";
                Vm = new CompanyDetailsDto();
                return;
            }

            // Mitarbeiter listesi
            var (okEmployees, employees, employeesMsg) =
                await _api.GetAsync<IEnumerable<EmployeeDto>>($"/api/Company/{id}/Employees");

            var employeeList = okEmployees && employees is not null
                ? employees
                : Array.Empty<EmployeeDto>();

            // UI ViewModel’e map et
            Vm = ApiToUiMapper.BuildDetails(company, employeeList);
        }
    }
}
