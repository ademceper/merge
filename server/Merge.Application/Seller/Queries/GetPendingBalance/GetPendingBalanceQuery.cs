using MediatR;

namespace Merge.Application.Seller.Queries.GetPendingBalance;

public record GetPendingBalanceQuery(
    Guid SellerId
) : IRequest<decimal>;
