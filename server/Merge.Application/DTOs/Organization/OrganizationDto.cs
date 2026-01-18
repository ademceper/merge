using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Organization;

/// <summary>
/// Organization DTO - Immutable record
/// </summary>
public record OrganizationDto(
    Guid Id,
    string Name,
    string? LegalName,
    string? TaxNumber,
    string? RegistrationNumber,
    string? Email,
    string? Phone,
    string? Website,
    string? Address,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string Status,
    bool IsVerified,
    DateTime? VerifiedAt,
    /// <summary>
    /// Organizasyon ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    OrganizationSettingsDto? Settings,
    int UserCount,
    int TeamCount,
    DateTime CreatedAt);
