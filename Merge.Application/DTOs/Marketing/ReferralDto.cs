using Merge.Domain.Modules.Marketing;
namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Referral DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record ReferralDto(
    Guid Id,
    string ReferredUserEmail,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    int PointsAwarded);
