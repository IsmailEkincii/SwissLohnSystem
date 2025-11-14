namespace SwissLohnSystem.API.DTOs.Setting;

public sealed class SettingDto
{
    public int Id { get; set; }                 // <-- EKLİ
    public string Name { get; set; } = null!;
    public decimal Value { get; set; }
    public string? Description { get; set; }
}
