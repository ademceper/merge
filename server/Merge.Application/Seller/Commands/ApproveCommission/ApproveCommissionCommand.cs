using MediatR;

namespace Merge.Application.Seller.Commands.ApproveCommission;

public record ApproveCommissionCommand(
    Guid CommissionId
) : IRequest<bool>;
