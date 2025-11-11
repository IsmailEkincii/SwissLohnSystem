using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SwissLohnSystem.UI.Pages.Companies.Employees
{
    public class CreateModel : PageModel, IValidatableObject
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CreateModel(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public int CompanyId { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public void OnGet()
        {
            Input.CompanyId = CompanyId;
            Input.Active = true; // default
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (CompanyId <= 0) { TempData["Error"] = "Ungültige Firmen-ID."; return RedirectToPage("/Companies/Index"); }

            // Custom validation da çalýþsýn
            if (!ModelState.IsValid) return Page();

            var api = _httpClientFactory.CreateClient("ApiClient");

            try
            {
                var salaryType = Input.SalaryTypeOption == "Stundenlohn" ? "Hourly" : "Monthly";

                var dto = new EmployeePostDto
                {
                    CompanyId = Input.CompanyId,
                    FirstName = Input.FirstName!.Trim(),
                    LastName = Input.LastName!.Trim(),
                    Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                    Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position!.Trim(),
                    BirthDate = Input.BirthDate,
                    MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus!,
                    ChildCount = Input.ChildCount ?? 0,
                    SalaryType = salaryType,
                    HourlyRate = Input.HourlyRate ?? 0,
                    MonthlyHours = Input.MonthlyHours ?? 0,
                    BruttoSalary = Input.BruttoSalary ?? 0,
                    StartDate = Input.StartDate!.Value,
                    EndDate = Input.EndDate,
                    WorkedHours = 0,
                    OvertimeHours = 0,
                    Active = Input.Active
                };

                var res = await api.PostAsJsonAsync("/api/Employee", dto);

                ApiResponse<EmployeeDto>? payload = null;
                string? raw = null;
                try { payload = await res.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>(); }
                catch { raw = await res.Content.ReadAsStringAsync(); }

                if (res.IsSuccessStatusCode && payload?.Success == true && payload.Data is not null)
                {
                    TempData["Alert"] = payload.Message ?? "Mitarbeiter wurde erfolgreich hinzugefügt.";
                    return RedirectToPage("/Companies/Details", new { id = CompanyId });
                }

                var err = payload?.Message ?? raw ?? "Unbekannter Fehler.";
                TempData["Error"] = $"Speichern fehlgeschlagen: {err}";
                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Fehler: {ex.Message}";
                return Page();
            }
        }

        // ----- Validation (Almanca mesajlar + koþullu alanlar) -----
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Koþullu zorunluluklar
            if (Input.SalaryTypeOption == "Stundenlohn")
            {
                if (Input.HourlyRate is null or <= 0)
                    yield return new ValidationResult("Stundenlohn ist erforderlich und muss größer als 0 sein.", new[] { nameof(Input.HourlyRate) });
            }
            else if (Input.SalaryTypeOption == "Monatslohn")
            {
                if (Input.BruttoSalary is null or <= 0)
                    yield return new ValidationResult("Bruttolohn ist erforderlich und muss größer als 0 sein.", new[] { nameof(Input.BruttoSalary) });
            }
        }

        // ----- Form Model -----
        public class InputModel
        {
            [Required] public int CompanyId { get; set; }

            [Required(ErrorMessage = "Vorname ist erforderlich.")]
            [StringLength(150, ErrorMessage = "Vorname darf maximal 150 Zeichen lang sein.")]
            public string? FirstName { get; set; }

            [Required(ErrorMessage = "Nachname ist erforderlich.")]
            [StringLength(150, ErrorMessage = "Nachname darf maximal 150 Zeichen lang sein.")]
            public string? LastName { get; set; }

            [EmailAddress(ErrorMessage = "Ungültige E-Mail-Adresse.")]
            public string? Email { get; set; }

            public string? Position { get; set; }

            public DateTime? BirthDate { get; set; }

            public string? MaritalStatus { get; set; } // Single / Married

            [Range(0, 20, ErrorMessage = "Anzahl Kinder muss zwischen 0 und 20 sein.")]
            public int? ChildCount { get; set; }

            [Required(ErrorMessage = "Gehaltsart ist erforderlich.")]
            [RegularExpression("Monatslohn|Stundenlohn", ErrorMessage = "Ungültige Gehaltsart.")]
            public string? SalaryTypeOption { get; set; } // UI seçimi (B: Monatslohn/Stundenlohn)

            [Range(0, 1000, ErrorMessage = "Stundenlohn muss zwischen 0 und 1000 liegen.")]
            public decimal? HourlyRate { get; set; }

            [Range(0, 300, ErrorMessage = "Monatsstunden müssen zwischen 0 und 300 liegen.")]
            public int? MonthlyHours { get; set; }

            [Range(0, 1000000, ErrorMessage = "Bruttolohn muss zwischen 0 und 1'000'000 liegen.")]
            public decimal? BruttoSalary { get; set; }

            [Required(ErrorMessage = "Startdatum ist erforderlich.")]
            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public bool Active { get; set; } = true;
        }

        // ----- API DTO'larý -----
        public class EmployeePostDto
        {
            public int CompanyId { get; set; }
            public string FirstName { get; set; } = null!;
            public string LastName { get; set; } = null!;
            public string? Email { get; set; }
            public string? Position { get; set; }
            public DateTime? BirthDate { get; set; }
            public string? MaritalStatus { get; set; }
            public int ChildCount { get; set; }
            public string SalaryType { get; set; } = null!; // "Monthly" | "Hourly"
            public decimal HourlyRate { get; set; }
            public int MonthlyHours { get; set; }
            public decimal BruttoSalary { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public int WorkedHours { get; set; }
            public int OvertimeHours { get; set; }
            public bool Active { get; set; }
        }

        public record EmployeeDto(
            int Id, int CompanyId,
            string FirstName, string LastName,
            string? Email, string? Position,
            DateTime BirthDate, string MaritalStatus, int ChildCount,
            string SalaryType, decimal HourlyRate, int MonthlyHours, decimal BruttoSalary,
            DateTime StartDate, DateTime? EndDate, int WorkedHours, int OvertimeHours, bool Active
        );

        public class ApiResponse<T>
        {
            [JsonPropertyName("success")] public bool Success { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public T? Data { get; set; }
        }
    }
}
