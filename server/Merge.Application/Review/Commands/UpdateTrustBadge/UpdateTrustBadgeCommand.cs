using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.UpdateTrustBadge;

public record UpdateTrustBadgeCommand(
    Guid BadgeId,
    string? Name,
    string? Description,
    string? IconUrl,
    string? BadgeType,
    TrustBadgeSettingsDto? Criteria,
    bool? IsActive,
    int? DisplayOrder,
    string? Color
) : IRequest<TrustBadgeDto>;
