using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.RequestPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RequestPayoutCommand(
    Guid SellerId,
    List<Guid> CommissionIds
) : IRequest<CommissionPayoutDto>;
