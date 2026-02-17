namespace SwissLohnSystem.API.DTOs.Setting
{
    public sealed class BvgPlanListItemDto
    {
        public string Code { get; set; } = null!; // PlanCode
        public int Year { get; set; }
        public string? Name { get; set; }         // PlanBaseCode (API Name alanına koymuş)
    }
}
