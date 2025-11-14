using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Services.Payroll
{
    public interface ISettingsProvider
    {
        PayrollSettingsSnapshot GetEffectiveSettings(string canton);

        // 💡 Yeni: QST tarifini döndür
        QstTariff? GetQstTariff(
            string canton,
            string? code,
            string permitType,
            bool churchMember,
            decimal grossMonthly);
    }
}
