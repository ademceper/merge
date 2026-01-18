using MediatR;

namespace Merge.Application.Seller.Queries.GetAvailablePayoutAmount;

public record GetAvailablePayoutAmountQuery(
    Guid SellerId
) : IRequest<decimal>;
