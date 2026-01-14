using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerBalance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerBalanceQuery(
    Guid SellerId
) : IRequest<SellerBalanceDto>;
