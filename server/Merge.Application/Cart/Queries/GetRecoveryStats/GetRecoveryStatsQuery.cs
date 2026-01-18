using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetRecoveryStats;

public record GetRecoveryStatsQuery(int Days = 30) : IRequest<AbandonedCartRecoveryStatsDto>;

