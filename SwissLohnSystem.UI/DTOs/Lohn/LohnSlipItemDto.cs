namespace SwissLohnSystem.UI.DTOs.Lohn
{
    public enum LohnSlipGroup
    {
        Earnings = 0,
        DeductionsEmployee = 1,
        ContributionsEmployer = 2
    }

    public class LohnSlipItemDto
    {
        public LohnSlipGroup Group { get; set; }
        public string Title { get; set; } = "";
        public string Side { get; set; } = "—"; // "AN" / "AG" / "—"

        // ✅ Yeni kolonlar
        public decimal? Base { get; set; }   // Basis
        public decimal? Rate { get; set; }   // Satz (%)
        public string? RateText { get; set; } // ✅ QST gibi metin oranlar için (örn: "A0Y / ZH")
        public decimal Amount { get; set; }
        public int SortOrder { get; set; }
        public decimal? Quantity { get; set; }   // Menge
        public decimal? UnitRate { get; set; }   // Ansatz
        public string? Unit { get; set; }        // h / Tag / Stück

    }
}
