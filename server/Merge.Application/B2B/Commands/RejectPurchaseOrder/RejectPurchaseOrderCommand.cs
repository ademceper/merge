using MediatR;

namespace Merge.Application.B2B.Commands.RejectPurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RejectPurchaseOrderCommand(
    Guid Id,
    string Reason
) : IRequest<bool>;

