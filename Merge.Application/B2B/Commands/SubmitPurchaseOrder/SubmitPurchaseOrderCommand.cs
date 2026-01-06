using MediatR;

namespace Merge.Application.B2B.Commands.SubmitPurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SubmitPurchaseOrderCommand(Guid Id) : IRequest<bool>;

