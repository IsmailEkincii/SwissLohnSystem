namespace SwissLohnSystem.UI.DTOs.Payroll
{
    public sealed class BvgPlanListItemDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int? Year { get; set; }
        public string Display => Year.HasValue ? $"{Name} ({Year})" : Name;
    }
}
