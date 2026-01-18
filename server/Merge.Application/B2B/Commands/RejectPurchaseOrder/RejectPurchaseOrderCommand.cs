using MediatR;

namespace Merge.Application.B2B.Commands.RejectPurchaseOrder;

public record RejectPurchaseOrderCommand(
    Guid Id,
    string Reason
) : IRequest<bool>;

