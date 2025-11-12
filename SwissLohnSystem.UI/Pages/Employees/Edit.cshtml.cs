using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SwissLohnSystem.UI.Services;

namespace SwissLohnSystem.UI.Pages.Employees
{
    public class EditModel : PageModel, IValidatableObject
    {
        private readonly ApiClient _api;
        public EditModel(ApiClient api) => _api = api;

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        [BindProperty(SupportsGet = true)] public int CompanyId { get; set; }

        [BindProperty] public InputModel Input { get; set; } = new();

        [TempData] public string? Toast { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;

            var (ok, emp, msg) = await _api.GetAsync<EmployeeDtoFromApi>($"/api/Employee/{id}");
            if (!ok || emp is null)
            {
                TempData["Error"] = msg ?? "Mitarbeiter wurde nicht gefunden.";
                return RedirectToPage("/Companies/Index");
            }

            CompanyId = emp.CompanyId;

            Input = new InputModel
            {
                Id = emp.Id,
                CompanyId = emp.CompanyId,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                Email = emp.Email,
                Position = emp.Position,
                BirthDate = emp.BirthDate,
                MaritalStatus = emp.MaritalStatus,
                ChildCount = emp.ChildCount,
                SalaryTypeOption = emp.SalaryType == "Hourly" ? "Stundenlohn" : "Monatslohn",
                HourlyRate = emp.HourlyRate,
                MonthlyHours = emp.MonthlyHours,
                BruttoSalary = emp.BruttoSalary,
                StartDate = emp.StartDate,
                EndDate = emp.EndDate,
                Active = emp.Active
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var salaryType = Input.SalaryTypeOption == "Stundenlohn" ? "Hourly" : "Monthly";

            var body = new EmployeeUpdateDtoForApi
            {
                Id = Input.Id,
                CompanyId = Input.CompanyId,
                FirstName = Input.FirstName!.Trim(),
                LastName = Input.LastName!.Trim(),
                Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email!.Trim(),
                Position = string.IsNullOrWhiteSpace(Input.Position) ? null : Input.Position!.Trim(),
                BirthDate = Input.BirthDate,
                MaritalStatus = string.IsNullOrWhiteSpace(Input.MaritalStatus) ? null : Input.MaritalStatus!,
                ChildCount = Input.ChildCount ?? 0,
                SalaryType = salaryType,
                HourlyRate = Input.HourlyRate ?? 0m,
                MonthlyHours = Input.MonthlyHours ?? 0,
                BruttoSalary = Input.BruttoSalary ?? 0m,
                StartDate = Input.StartDate!.Value,
                EndDate = Input.EndDate,
                Active = Input.Active,

                // opsiyoneller alanlar (þimdilik dokunmuyoruz):
                AHVNumber = null,
                Krankenkasse = null,
                BVGPlan = null,
                PensumPercent = null,
                HolidayRate = null,
                OvertimeRate = null,
                WithholdingTaxCode = null,
                Address = null,
                Zip = null,
                City = null,
                Phone = null
            };

            var (ok, _, msg) = await _api.PutAsync<string>($"/api/Employee/{Input.Id}", body);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, msg ?? "Aktualisierung fehlgeschlagen.");
                return Page();
            }

            TempData["Alert"] = "Mitarbeiter erfolgreich aktualisiert.";
            return RedirectToPage("/Companies/Details", new { id = Input.CompanyId });
        }

        // ----- ViewModel -----
        public class InputModel
        {
            [Required] public int Id { get; set; }
            [Required] public int CompanyId { get; set; }

            [Required, StringLength(150)] public string? FirstName { get; set; }
            [Required, StringLength(150)] public string? LastName { get; set; }
            [EmailAddress] public string? Email { get; set; }
            public string? Position { get; set; }

            public DateTime? BirthDate { get; set; }
            public string? MaritalStatus { get; set; }
            [Range(0, 20)] public int? ChildCount { get; set; }

            [Required, RegularExpression("Monatslohn|Stundenlohn")] public string? SalaryTypeOption { get; set; }
            [Range(0, 1000)] public decimal? HourlyRate { get; set; }
            [Range(0, 300)] public int? MonthlyHours { get; set; }
            [Range(0, 1000000)] public decimal? BruttoSalary { get; set; }

            [Required] public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public bool Active { get; set; }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
        {
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

            if (Input.EndDate.HasValue && Input.StartDate.HasValue && Input.EndDate < Input.StartDate)
                yield return new ValidationResult("Enddatum darf nicht vor dem Startdatum liegen.", new[] { nameof(Input.EndDate) });
        }

        // ----- API DTO’larý -----
        public record EmployeeDtoFromApi(
            int Id, int CompanyId,
            string FirstName, string LastName,
            string? Email, string? Position,
            DateTime? BirthDate, string? MaritalStatus, int ChildCount,
            string SalaryType, decimal HourlyRate, int MonthlyHours, decimal BruttoSalary,
            DateTime StartDate, DateTime? EndDate, bool Active,
            string? AHVNumber, string? Krankenkasse, string? BVGPlan,
            decimal? PensumPercent, decimal? HolidayRate, decimal? OvertimeRate, string? WithholdingTaxCode,
            string? Address, string? Zip, string? City, string? Phone
        );

        public class EmployeeUpdateDtoForApi
        {
            public int Id { get; set; }
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
            public bool Active { get; set; }

            public string? AHVNumber { get; set; }
            public string? Krankenkasse { get; set; }
            public string? BVGPlan { get; set; }
            public decimal? PensumPercent { get; set; }
            public decimal? HolidayRate { get; set; }
            public decimal? OvertimeRate { get; set; }
            public string? WithholdingTaxCode { get; set; }
            public string? Address { get; set; }
            public string? Zip { get; set; }
            public string? City { get; set; }
            public string? Phone { get; set; }
        }
    }
}
