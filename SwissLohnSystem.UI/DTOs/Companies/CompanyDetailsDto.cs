using System.Collections.Generic;
using SwissLohnSystem.API.DTOs.Setting;
using SwissLohnSystem.UI.DTOs.Employees; // EmployeeDto burada varsa

namespace SwissLohnSystem.UI.DTOs.Companies
{
    /// <summary>
    /// Companies/Details sayfası için birleşik DTO (UI ViewModel).
    /// </summary>
    public class CompanyDetailsDto
    {
        public CompanyDto Company { get; set; } = new();
        public IEnumerable<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
        // İleride: public IEnumerable<LohnMonthlyRowDto> Lohns { get; set; }
        public string? DefaultBvgPlanCode { get; set; }
        public List<SettingDto> Settings { get; set; } = new();

    }
}
