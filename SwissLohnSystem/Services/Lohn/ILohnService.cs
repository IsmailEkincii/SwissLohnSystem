using SwissLohnSystem.API.DTOs.Lohn;
using SwissLohnSystem.API.DTOs.Payroll;

namespace SwissLohnSystem.API.Services.Lohn
{
    public interface ILohnService
    {
        Task<LohnDto> CalculateAsync(PayrollRequestDto request, CancellationToken ct = default);
        Task FinalizeAsync(int lohnId, CancellationToken ct = default);

        // ✅ FIX: şirketin bir ayındaki Lohn satırları
        Task<List<CompanyMonthlyLohnDto>> GetCompanyMonthlyAsync(int companyId, int year, int month, CancellationToken ct = default);

        Task<LohnDetailsDto> GetDetailsAsync(int lohnId, CancellationToken ct = default);
        // ✅ NEW
        Task<LohnausweisDto> GetLohnausweisAsync(int employeeId, int year, CancellationToken ct = default);
    }
}
