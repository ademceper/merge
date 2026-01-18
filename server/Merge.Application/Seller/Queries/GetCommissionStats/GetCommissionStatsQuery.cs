using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetCommissionStats;

public record GetCommissionStatsQuery(
    Guid? SellerId = null
) : IRequest<CommissionStatsDto>;
