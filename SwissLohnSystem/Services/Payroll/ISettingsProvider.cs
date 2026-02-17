using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Services.Payroll
{
    public interface ISettingsProvider
    {
        EffectivePayrollSettings GetEffectiveSettings(int companyId, string canton, string? bvgPlanCode);
        QstTariff? GetQstTariff(int companyId, string canton, string code, string permitType, bool churchMember, decimal income);
    }
}
