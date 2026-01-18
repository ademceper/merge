using MediatR;

namespace Merge.Application.B2B.Commands.ApprovePurchaseOrder;

public record ApprovePurchaseOrderCommand(
    Guid Id,
    Guid ApprovedByUserId
) : IRequest<bool>;

