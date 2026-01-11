using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPayoutQuery(
    Guid PayoutId
) : IRequest<CommissionPayoutDto?>;
