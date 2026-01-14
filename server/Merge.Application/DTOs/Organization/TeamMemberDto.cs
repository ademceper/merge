using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.Organization;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// <summary>
/// Team Member DTO - Immutable record
/// </summary>
public record TeamMemberDto(
    Guid Id,
    Guid TeamId,
    string TeamName,
    Guid UserId,
    string UserName,
    string UserEmail,
    string Role,
    DateTime JoinedAt,
    bool IsActive);
