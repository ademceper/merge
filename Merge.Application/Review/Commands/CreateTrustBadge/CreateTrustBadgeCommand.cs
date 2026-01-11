using MediatR;
using Merge.Application.DTOs.Review;

namespace Merge.Application.Review.Commands.CreateTrustBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateTrustBadgeCommand(
    string Name,
    string Description,
    string IconUrl,
    string BadgeType,
    TrustBadgeSettingsDto? Criteria,
    bool IsActive,
    int DisplayOrder,
    string? Color
) : IRequest<TrustBadgeDto>;
