using Microsoft.AspNetCore.Mvc;
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
        public DetailsModel(ApiClient api) => _api = api;

        public EmployeeDto? Employee { get; private set; }
        public CompanyDto? Company { get; private set; }
        public List<LohnDto> LohnList { get; private set; } = new();
        public string? LoadError { get; private set; }
        public string DefaultPeriod { get; private set; } = $"{DateTime.Today:yyyy-MM}";

        public async Task OnGetAsync(int id)
        {
            var empRes = await _api.GetAsync<EmployeeDto>($"/api/Employee/{id}");
            if (!empRes.ok || empRes.data is null)
            {
                LoadError = empRes.message ?? "Mitarbeiter konnte nicht geladen werden.";
                return;
            }
            Employee = empRes.data;

            var compRes = await _api.GetAsync<CompanyDto>($"/api/Company/{Employee.CompanyId}");
            if (compRes.ok && compRes.data is not null)
                Company = compRes.data;

            var byEmpRes = await _api.GetAsync<IEnumerable<LohnDto>>($"/api/Lohn/by-employee/{id}");
            if (byEmpRes.ok && byEmpRes.data is not null)
            {
                LohnList = byEmpRes.data
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Month)
                    .ToList();
            }
        }

        // Lohn berechnen (server-side POST)
        public class CalcInput { public string? Period { get; set; } } // YYYY-MM
        [BindProperty] public CalcInput Input { get; set; } = new();

        public async Task<IActionResult> OnPostCalcAsync(int id)
        {
            var empRes = await _api.GetAsync<EmployeeDto>($"/api/Employee/{id}");
            if (!empRes.ok || empRes.data is null)
            {
                LoadError = empRes.message ?? "Mitarbeiter wurde nicht gefunden.";
                await OnGetAsync(id);
                return Page();
            }
            Employee = empRes.data;

            var p = (Input.Period ?? "").Trim();
            if (p.Length != 7 || p[4] != '-' ||
                !int.TryParse(p.AsSpan(0, 4), out var year) ||
                !int.TryParse(p.AsSpan(5, 2), out var month) ||
                month < 1 || month > 12)
            {
                LoadError = "Ungültiger Zeitraum. Erwartet: YYYY-MM.";
                await OnGetAsync(id);
                return Page();
            }

            var body = new LohnCalculateDto { EmployeeId = id, Year = year, Month = month };
            var calcRes = await _api.PostAsync<LohnDto>("/api/Lohn/calc", body);
            if (!calcRes.ok)
            {
                LoadError = calcRes.message ?? "Berechnung fehlgeschlagen.";
                await OnGetAsync(id);
                return Page();
            }

            // Baþarýlý -> listeyi yenile
            await OnGetAsync(id);
            return Page();
        }
    }
}
