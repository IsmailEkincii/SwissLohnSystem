using System.Threading.Tasks;
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
        public DetailsModel(ApiClient api) => _api = api;

        public int Id { get; set; }
        public LohnDetailsDto? Vm { get; set; }

        public async Task OnGetAsync(int id)
        {
            Id = id;

            var (okL, l, msgL) = await _api.GetAsync<LohnDto>($"/api/Lohn/{id}");
            if (!okL || l is null) { ViewData["Error"] = msgL ?? "Lohn konnte nicht geladen werden."; return; }

            // Çalýþaný getir (firma bilgisi için)
            var (okE, e, _) = await _api.GetAsync<EmployeeDto>($"/api/Employee/{l.EmployeeId}");

            // (Ýsteðe baðlý) þirketi getir
            CompanyDto? c = null;
            if (okE && e is not null)
            {
                var (okC, cdata, _) = await _api.GetAsync<CompanyDto>($"/api/Company/{e.CompanyId}");
                if (okC) c = cdata;
            }

            Vm = l.ToDetails(e, c);
        }
    }
}
