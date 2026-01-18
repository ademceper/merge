using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetLowStockProducts;

public record GetLowStockProductsQuery(
    int Threshold
) : IRequest<List<LowStockProductDto>>;

