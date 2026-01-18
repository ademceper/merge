using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeRecommendation;

public record GetSizeRecommendationQuery(
    Guid ProductId,
    decimal Height,
    decimal Weight,
    decimal? Chest = null,
    decimal? Waist = null
) : IRequest<SizeRecommendationDto>;
