using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetRecoveryStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetRecoveryStatsQuery(int Days = 30) : IRequest<AbandonedCartRecoveryStatsDto>;

