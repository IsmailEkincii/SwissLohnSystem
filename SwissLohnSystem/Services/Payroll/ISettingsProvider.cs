namespace SwissLohnSystem.API.Services.Payroll
{
    public interface ISettingsProvider
    {
        PayrollSettingsSnapshot GetEffectiveSettings(string canton);
    }
}
