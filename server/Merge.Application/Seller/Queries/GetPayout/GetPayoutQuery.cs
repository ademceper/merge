using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetPayout;

public record GetPayoutQuery(
    Guid PayoutId
) : IRequest<CommissionPayoutDto?>;
