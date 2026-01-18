using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.CreateTrustBadge;

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
