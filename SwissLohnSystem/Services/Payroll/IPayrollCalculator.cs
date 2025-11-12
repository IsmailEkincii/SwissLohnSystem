using SwissLohnSystem.API.DTOs.Payroll;

namespace SwissLohnSystem.API.Services.Payroll
{
    public interface IPayrollCalculator
    {
        PayrollResponseDto Calculate(PayrollRequestDto req);
    }
}
