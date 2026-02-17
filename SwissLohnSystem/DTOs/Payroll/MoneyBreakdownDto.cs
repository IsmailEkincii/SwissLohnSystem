namespace SwissLohnSystem.API.DTOs.Payroll
{
    public sealed class MoneyBreakdownDto
    {
        // AHV/IV/EO
        public decimal AHV_IV_EO { get; set; }

        // ALV total (ALV1 + ALV2)
        public decimal ALV { get; set; }

        // ✅ UVG split (Excel/UI uyumu)
        // Employee tarafında genelde NBU (AN)
        public decimal UVG_NBU { get; set; }

        // Employer tarafında genelde BU (AG)
        public decimal UVG_BU { get; set; }

        // BVG (Fix AN/AG)
        public decimal BVG { get; set; }

        // KTG (gender-based, AN/AG)
        public decimal KTG { get; set; }

        // QST (sadece employee)
        public decimal WithholdingTax { get; set; }

        // Diğer (canteen vb.)
        public decimal Other { get; set; }
    }
}
