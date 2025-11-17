using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Employees;
using SwissLohnSystem.UI.DTOs.Lohn;
using SwissLohnSystem.UI.Services;
using SwissLohnSystem.UI.Services.Mapping; // ToDetails uzantýsý varsa

namespace SwissLohnSystem.UI.Pages.Lohn
{
    public class DetailsModel : PageModel
    {
        private readonly ApiClient _api;
        public DetailsModel(ApiClient api) => _api = api;

        public int Id { get; set; }
        public LohnDetailsDto? Vm { get; set; }

        public async Task OnGetAsync(int id)
        {
            Id = id;

            // ?? JS için API base URL
            ViewData["ApiBaseUrl"] = _api.BaseUrl?.TrimEnd('/');

            var (okL, l, msgL) = await _api.GetAsync<LohnDto>($"/api/Lohn/{id}");
            if (!okL || l is null)
            {
                ViewData["Error"] = msgL ?? "Lohn konnte nicht geladen werden.";
                return;
            }

            var (okE, e, _) = await _api.GetAsync<EmployeeDto>($"/api/Employee/{l.EmployeeId}");
            CompanyDto? c = null;
            if (okE && e is not null)
            {
                var (okC, cdata, _) = await _api.GetAsync<CompanyDto>($"/api/Company/{e.CompanyId}");
                if (okC) c = cdata;
            }

            Vm = l.ToDetails(e, c); // ya da basit mapping: Vm = new LohnDetailsDto { ... }
        }
    }
}
