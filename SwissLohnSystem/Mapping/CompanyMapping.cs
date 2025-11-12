using SwissLohnSystem.API.DTOs.Companies;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mappings
{
    public static class CompanyMappings
    {
        public static CompanyDto ToDto(this Company c) =>
            new CompanyDto(
                c.Id,
                c.Name,
                c.Address,
                c.Canton,
                c.Email,
                c.Phone,
                c.TaxNumber
            );

        public static Company ToEntity(this CompanyCreateDto dto) =>
            new Company
            {
                Name = dto.Name.Trim(),
                Canton = dto.Canton.Trim(),
                Address = dto.Address?.Trim(),
                Email = dto.Email?.Trim(),
                Phone = dto.Phone?.Trim(),
                TaxNumber = dto.TaxNumber?.Trim()
            };

        public static void Apply(this Company entity, CompanyUpdateDto dto)
        {
            entity.Name = dto.Name.Trim();
            entity.Canton = dto.Canton.Trim();
            entity.Address = dto.Address?.Trim();
            entity.Email = dto.Email?.Trim();
            entity.Phone = dto.Phone?.Trim();
            entity.TaxNumber = dto.TaxNumber?.Trim();
        }
    }
}
