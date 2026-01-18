using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Search.Queries.GetNewArrivals;

public record GetNewArrivalsQuery(
    int Days = 30,
    int MaxResults = 10
) : IRequest<IReadOnlyList<ProductRecommendationDto>>;
