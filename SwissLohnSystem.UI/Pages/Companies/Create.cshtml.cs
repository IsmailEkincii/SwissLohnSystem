using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.DTOs.Payroll;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class CreateModel : PageModel
    {
        private readonly ApiClient _api;
        public CreateModel(ApiClient api) => _api = api;

        public IReadOnlyList<string> Cantons { get; } = new[]
        {
            "ZH","BE","LU","UR","SZ","OW","NW","GL","ZG",
            "FR","SO","BS","BL","SH","AR","AI","SG","GR",
            "AG","TG","TI","VD","VS","NE","GE","JU"
        };

        public List<SelectListItem> BvgPlans { get; private set; } = new();

        [BindProperty] public InputModel Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadBvgPlansAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadBvgPlansAsync();

            if (!ModelState.IsValid) return Page();

            var body = new CompanyCreateDto
            {
                Name = Input.Name!.Trim(),
                Canton = Input.Canton!,
                Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address!.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone!.Trim(),
                TaxNumber = string.IsNullOrWhiteSpace(Input.TaxNumber) ? null : Input.TaxNumber!.Trim(),
                DefaultBvgPlanCode = string.IsNullOrWhiteSpace(Input.DefaultBvgPlanCode) ? null : Input.DefaultBvgPlanCode.Trim()
            };

            try
            {
                var (ok, data, message) = await _api.PostAsync<CompanyDto>("/api/Company", body);

                if (ok && data is not null)
                {
                    TempData["Toast"] = message ?? "Firma wurde erfolgreich hinzugefügt.";
                    return RedirectToPage("/Companies/Details", new { id = data.Id });
                }

                TempData["Error"] = message ?? "Speichern fehlgeschlagen.";
                return Page();

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Fehler: {ex.Message}";
                return Page();
            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Name ist erforderlich.")]
            [StringLength(200, ErrorMessage = "Name darf maximal 200 Zeichen lang sein.")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Kanton ist erforderlich.")]
            [RegularExpression("ZH|BE|LU|UR|SZ|OW|NW|GL|ZG|FR|SO|BS|BL|SH|AR|AI|SG|GR|AG|TG|TI|VD|VS|NE|GE|JU",
                ErrorMessage = "Ungültiger Kanton.")]
            public string? Canton { get; set; }

            [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
            public string? Email { get; set; }

            public string? Address { get; set; }
            public string? Phone { get; set; }
            public string? TaxNumber { get; set; }

            public string? DefaultBvgPlanCode { get; set; }
        }

        private async Task LoadBvgPlansAsync()
        {
            var (ok, data, msg) = await _api.GetAsync<List<BvgPlanListItemDto>>("/api/Settings/bvg-plans");

            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- (kein Default) --" }
            };

            if (ok && data is not null)
            {
                foreach (var p in data)
                {
                    var text = p.Code;
                    if (p.Year.HasValue) text = $"{p.Code} ({p.Year})";
                    list.Add(new SelectListItem { Value = p.Code, Text = text });
                }
            }

            BvgPlans = list;
        }
    }
}
