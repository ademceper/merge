using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopProducts;

public record GetTopProductsQuery(
    DateTime StartDate,
    DateTime EndDate,
    int Limit
) : IRequest<List<TopProductDto>>;

