using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.Organization;

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
