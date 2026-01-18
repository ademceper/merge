using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetStoreStats;

public record GetStoreStatsQuery(
    Guid StoreId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<StoreStatsDto>;
