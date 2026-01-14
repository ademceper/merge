using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.User;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record AddressDto(
    Guid Id,
    string Title,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string District,
    string PostalCode,
    string Country,
    bool IsDefault);

