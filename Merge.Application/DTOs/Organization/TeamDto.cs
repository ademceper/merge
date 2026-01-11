namespace Merge.Application.DTOs.Organization;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// <summary>
/// Team DTO - Immutable record
/// </summary>
public record TeamDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Name,
    string? Description,
    Guid? TeamLeadId,
    string? TeamLeadName,
    bool IsActive,
    /// <summary>
    /// Takim ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    TeamSettingsDto? Settings,
    int MemberCount,
    DateTime CreatedAt);
