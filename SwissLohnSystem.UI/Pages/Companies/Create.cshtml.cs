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
    public class CreateModel : PageModel
    {
        private readonly ApiClient _api;
        public CreateModel(ApiClient api) => _api = api;

        // Ýsviçre kanton kýsaltmalarý
        public IReadOnlyList<string> Cantons { get; } = new[]
        {
            "ZH","BE","LU","UR","SZ","OW","NW","GL","ZG",
            "FR","SO","BS","BL","SH","AR","AI","SG","GR",
            "AG","TG","TI","VD","VS","NE","GE","JU"
        };

        [BindProperty] public InputModel Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var body = new CompanyCreateDto
            {
                Name = Input.Name!.Trim(),
                Canton = Input.Canton!,
                Address = string.IsNullOrWhiteSpace(Input.Address) ? null : Input.Address!.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                Phone = string.IsNullOrWhiteSpace(Input.Phone) ? null : Input.Phone!.Trim(),
                TaxNumber = string.IsNullOrWhiteSpace(Input.TaxNumber) ? null : Input.TaxNumber!.Trim()
            };

            try
            {
                // API: POST /api/Company -> ApiResponse<CompanyDto>
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

        // Form ViewModel (Almanca validasyonlar)
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
        }
    }
}
