using SwissLohnSystem.API.DTOs.Companies;
using SwissLohnSystem.API.Models;

namespace SwissLohnSystem.API.Mapping;

public static class CompanyMapping
{
    public static CompanyDto ToDto(this Company c) =>
        new(c.Id, c.Name, c.Address, c.Canton, c.Email, c.Phone, c.TaxNumber);

    public static Company ToEntity(this CompanyCreateDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
        Canton = dto.Canton,
        Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
        Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
        TaxNumber = string.IsNullOrWhiteSpace(dto.TaxNumber) ? null : dto.TaxNumber.Trim()
    };

    public static void Apply(this Company entity, CompanyUpdateDto dto)
    {
        entity.Name = dto.Name.Trim();
        entity.Canton = dto.Canton;
        entity.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
        entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        entity.TaxNumber = string.IsNullOrWhiteSpace(dto.TaxNumber) ? null : dto.TaxNumber.Trim();
    }
}
