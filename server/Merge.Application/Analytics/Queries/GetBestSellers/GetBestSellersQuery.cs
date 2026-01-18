using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetBestSellers;

public record GetBestSellersQuery(
    int Limit
) : IRequest<List<TopProductDto>>;

