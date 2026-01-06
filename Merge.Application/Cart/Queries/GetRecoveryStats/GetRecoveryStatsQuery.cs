using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetRecoveryStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRecoveryStatsQuery(int Days = 30) : IRequest<AbandonedCartRecoveryStatsDto>;

