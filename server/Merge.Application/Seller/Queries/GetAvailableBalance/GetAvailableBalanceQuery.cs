using MediatR;

namespace Merge.Application.Seller.Queries.GetAvailableBalance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAvailableBalanceQuery(
    Guid SellerId
) : IRequest<decimal>;
