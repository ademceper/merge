using MediatR;

namespace Merge.Application.B2B.Commands.CancelPurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CancelPurchaseOrderCommand(Guid Id) : IRequest<bool>;

