using MediatR;

namespace Merge.Application.Seller.Queries.GetPendingBalance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPendingBalanceQuery(
    Guid SellerId
) : IRequest<decimal>;
