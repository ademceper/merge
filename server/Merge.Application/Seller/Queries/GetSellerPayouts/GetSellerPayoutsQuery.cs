using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerPayouts;

public record GetSellerPayoutsQuery(
    Guid SellerId
) : IRequest<IEnumerable<CommissionPayoutDto>>;
