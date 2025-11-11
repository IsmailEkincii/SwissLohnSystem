using SwissLohnSystem.API.DTOs.Admin;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class AdminMapping
    {
        public static AdminDto ToDto(this Admin a) => new(a.Id, a.Username);

        public static Admin ToEntity(this AdminCreateDto dto) => new()
        {
            Username = dto.Username.Trim(),
            PasswordHash = dto.PasswordHash // gerçek hayatta hash burada değil, servis katmanında yapılmalı
        };

        public static void Apply(this Admin entity, AdminUpdateDto dto)
        {
            entity.Username = dto.Username.Trim();
            entity.PasswordHash = dto.PasswordHash;
        }
    }
}