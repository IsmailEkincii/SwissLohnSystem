// örnek: SwissLohnSystem.API.DTOs.Payroll.PayrollRequestDto
using SwissLohnSystem.API.DTOs.Payroll;

public sealed class PayrollRequestDto
{
    public int EmployeeId { get; set; }

    public DateOnly Period { get; set; }   // veya DateTime, sen nasıl tanımladıysan
    public string? Canton { get; set; }

    /// <summary>
    /// Bu alan UI’dan gelse bile, LohnController DB’deki Employee’den override etmeli.
    /// </summary>
    public decimal GrossMonthly { get; set; }

    // Employee -> Apply* flag’leri
    public bool ApplyAHV { get; set; }
    public bool ApplyALV { get; set; }
    public bool ApplyBVG { get; set; }
    public bool ApplyNBU { get; set; }
    public bool ApplyBU { get; set; }
    public bool ApplyFAK { get; set; }
    public bool ApplyQST { get; set; }

    public decimal WeeklyHours { get; set; }

    public string? PermitType { get; set; }
    public string? WithholdingTaxCode { get; set; }
    public bool ChurchMember { get; set; }

    public BvgPlanDto? BvgPlan { get; set; }  // Zaten kullanıyorsun
}
