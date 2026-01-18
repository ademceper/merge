using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Commands.RequestPayout;

public record RequestPayoutCommand(
    Guid SellerId,
    List<Guid> CommissionIds
) : IRequest<CommissionPayoutDto>;
