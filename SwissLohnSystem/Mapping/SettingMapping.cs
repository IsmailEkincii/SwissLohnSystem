using SwissLohnSystem.API.DTOs.Setting;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class SettingMapping
    {
        public static SettingDto ToDto(this Setting s) => new()
        {
            Id = s.Id,
            CompanyId = s.CompanyId,
            Name = s.Name,
            Value = s.Value,
            Description = s.Description
        };

        public static void ApplyUpsert(this Setting s, SettingUpsertDto dto)
        {
            s.Name = dto.Name.Trim();
            s.Value = string.IsNullOrWhiteSpace(dto.Value) ? null : dto.Value.Trim();
            s.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            s.UpdatedAt = DateTime.UtcNow;
        }
    }
}
