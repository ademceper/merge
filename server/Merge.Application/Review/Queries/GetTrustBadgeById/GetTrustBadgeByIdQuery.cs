using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetTrustBadgeById;

public record GetTrustBadgeByIdQuery(
    Guid BadgeId
) : IRequest<TrustBadgeDto?>;
