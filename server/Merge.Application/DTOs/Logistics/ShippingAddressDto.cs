namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record ShippingAddressDto(
    Guid Id,
    Guid UserId,
    string Label,
    string FirstName,
    string LastName,
    string Phone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault,
    bool IsActive,
    string? Instructions,
    DateTime CreatedAt
);
