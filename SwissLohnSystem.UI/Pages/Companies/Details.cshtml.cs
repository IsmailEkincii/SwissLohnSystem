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
        public CompanyDetailsDto Vm { get; set; } = new();

        // 🔴 Swagger adresine göre BURASI:
        // https://localhost:7090/swagger/index.html => BaseUrl: https://localhost:7090
        public string ApiBaseUrl => "https://localhost:7090";

        public async Task OnGetAsync(int id)
        {
            Id = id;

            var companyRes = await _api.GetAsync<CompanyDto>($"/api/Company/{id}");
            var employeesRes = await _api.GetAsync<IEnumerable<EmployeeDto>>($"/api/Company/{id}/Employees");

            if (!companyRes.ok || companyRes.data is null)
            {
                ViewData["Error"] = companyRes.message ?? "Firma konnte nicht geladen werden.";
                Vm = new CompanyDetailsDto();
                return;
            }

            var company = companyRes.data;
            var employees = employeesRes.ok && employeesRes.data is not null
                ? employeesRes.data
                : Array.Empty<EmployeeDto>();

            Vm = ApiToUiMapper.BuildDetails(company!, employees!);
        }
    }
}
