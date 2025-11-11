using SwissLohnSystem.API.DTOs.Setting;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings;

public static class SettingMapping
{
    public static SettingDto ToDto(this Setting s) =>
        new(s.Id, s.Name, s.Value, s.Description);

    public static Setting ToEntity(this SettingCreateDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Value = dto.Value,
        Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim()
    };

    public static void Apply(this Setting entity, SettingUpdateDto dto)
    {
        entity.Name = dto.Name.Trim();
        entity.Value = dto.Value;
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
    }
}
