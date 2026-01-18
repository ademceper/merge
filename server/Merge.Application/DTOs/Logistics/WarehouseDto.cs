using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.Logistics;

public record WarehouseDto(
    Guid Id,
    string Name,
    string Code,
    string Address,
    string City,
    string Country,
    string PostalCode,
    string ContactPerson,
    string ContactPhone,
    string ContactEmail,
    int Capacity,
    bool IsActive,
    string? Description,
    DateTime CreatedAt
);
