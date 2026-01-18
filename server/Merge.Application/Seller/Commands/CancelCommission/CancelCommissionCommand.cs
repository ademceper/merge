using MediatR;

namespace Merge.Application.Seller.Commands.CancelCommission;

public record CancelCommissionCommand(
    Guid CommissionId
) : IRequest<bool>;
