using MediatR;

namespace Merge.Application.Seller.Queries.GetAvailablePayoutAmount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAvailablePayoutAmountQuery(
    Guid SellerId
) : IRequest<decimal>;
