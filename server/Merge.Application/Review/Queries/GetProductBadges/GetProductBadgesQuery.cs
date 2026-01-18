using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetProductBadges;

public record GetProductBadgesQuery(
    Guid ProductId
) : IRequest<IEnumerable<ProductTrustBadgeDto>>;
