using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Mapping;

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _api;
        public DetailsModel(ApiClient api) => _api = api;

        public int Id { get; set; }
        public CompanyDetailsDto Vm { get; set; } = new();

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

        // (Opsiyonel) Server-side Löhne yükleme örneði:
        public async Task<IEnumerable<LohnMonthlyRowDto>> LoadMonthlyAsync(int companyId, string period)
        {
            var loehneRes = await _api.GetAsync<IEnumerable<LohnDto>>($"/api/Lohn/by-company/{companyId}/monthly?period={period}");
            var employeesRes = await _api.GetAsync<IEnumerable<EmployeeDto>>($"/api/Company/{companyId}/Employees");
            if (!loehneRes.ok || !employeesRes.ok || loehneRes.data is null || employeesRes.data is null)
                return Array.Empty<LohnMonthlyRowDto>();

            return ApiToUiMapper.ToMonthlyRows(loehneRes.data, employeesRes.data);
        }
    }
}
