using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.DTOs.Companies;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Companies
{
    public class EditModel : PageModel
    {
        private readonly ApiClient _api;
        public EditModel(ApiClient api) => _api = api;

        // Ýsviçre kanton kýsaltmalarý
        public IReadOnlyList<string> Cantons { get; } = new[]
        {
            "ZH","BE","LU","UR","SZ","OW","NW","GL","ZG",
            "FR","SO","BS","BL","SH","AR","AI","SG","GR",
            "AG","TG","TI","VD","VS","NE","GE","JU"
        };

        [BindProperty] public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var (ok, data, message) = await _api.GetAsync<CompanyDto>($"/api/Company/{id}");
            if (!ok || data is null)
            {
                TempData["Error"] = message ?? "Firma wurde nicht gefunden.";
                return RedirectToPage("/Companies/Index");
            }

            Input = new InputModel
            {
                Id = data.Id,
                Name = data.Name,
                Canton = data.Canton,
                Address = data.Address,
                Email = data.Email,
                Phone = data.Phone,
                TaxNumber = data.TaxNumber
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var body = new CompanyUpdateDto
            {
                Id = Input.Id,
                Name = Input.Name!.Trim(),
                Canton = Input.Canton!,
                Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address!.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone!.Trim(),
                TaxNumber = string.IsNullOrWhiteSpace(Input.TaxNumber) ? null : Input.TaxNumber!.Trim()
            };

            var (ok, _, message) = await _api.PutAsync<string>($"/api/Company/{Input.Id}", body);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message ?? "Aktualisierung fehlgeschlagen.");
                return Page();
            }

            TempData["Toast"] = message ?? "Firma wurde erfolgreich aktualisiert.";
            return RedirectToPage("/Companies/Index");
        }

        public class InputModel
        {
            [Required] public int Id { get; set; }

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
        }
    }
}
