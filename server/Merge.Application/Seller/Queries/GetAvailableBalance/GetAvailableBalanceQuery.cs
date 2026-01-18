using MediatR;

namespace Merge.Application.Seller.Queries.GetAvailableBalance;

public record GetAvailableBalanceQuery(
    Guid SellerId
) : IRequest<decimal>;
