using MediatR;

namespace Merge.Application.B2B.Commands.ApprovePurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApprovePurchaseOrderCommand(
    Guid Id,
    Guid ApprovedByUserId
) : IRequest<bool>;

