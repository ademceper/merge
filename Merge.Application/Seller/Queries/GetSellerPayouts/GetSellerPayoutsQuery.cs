using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerPayouts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerPayoutsQuery(
    Guid SellerId
) : IRequest<IEnumerable<CommissionPayoutDto>>;
