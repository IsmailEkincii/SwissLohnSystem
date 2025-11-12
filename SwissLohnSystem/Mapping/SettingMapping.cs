using SwissLohnSystem.API.DTOs.Setting; // TEKİL (senin mevcut düzenine uygun)
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class SettingMapping
    {
        // Parametreli ctor’a gerek yok — object initializer kullan
        public static SettingDto ToDto(this Setting s) => new SettingDto
        {
            Id = s.Id,
            Name = s.Name,
            Value = s.Value,
            Description = s.Description
        };

        public static Setting ToEntity(this SettingCreateDto dto) => new Setting
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
}
