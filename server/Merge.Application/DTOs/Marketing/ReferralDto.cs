using Merge.Domain.Modules.Marketing;
namespace Merge.Application.DTOs.Marketing;


public record ReferralDto(
    Guid Id,
    string ReferredUserEmail,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int PointsAwarded);
