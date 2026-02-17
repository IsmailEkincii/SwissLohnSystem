namespace SwissLohnSystem.API.DTOs.Setting
{
    public class SettingDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Value { get; set; } = "";      // ✅ string
        public string? Description { get; set; }
    }

    public class SettingUpsertDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Value { get; set; } = "";         // ✅ string
        public string? Description { get; set; }
    }
}
