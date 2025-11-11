namespace SwissLohnSystem.API.DTOs.Setting;

public record SettingDto(
    int Id,
    string Name,
    decimal Value,
    string? Description
);
