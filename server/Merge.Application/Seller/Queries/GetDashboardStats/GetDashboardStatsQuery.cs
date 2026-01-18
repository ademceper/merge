using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetDashboardStats;

public record GetDashboardStatsQuery(
    Guid SellerId
) : IRequest<SellerDashboardStatsDto>;
