using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetDashboardStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetDashboardStatsQuery(
    Guid SellerId
) : IRequest<SellerDashboardStatsDto>;
