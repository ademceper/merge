using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerBalance;

public record GetSellerBalanceQuery(
    Guid SellerId
) : IRequest<SellerBalanceDto>;
