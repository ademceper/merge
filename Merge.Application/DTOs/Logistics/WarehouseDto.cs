namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
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
