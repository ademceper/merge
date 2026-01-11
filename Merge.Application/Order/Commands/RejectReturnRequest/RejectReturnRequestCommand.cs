using MediatR;

namespace Merge.Application.Order.Commands.RejectReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RejectReturnRequestCommand(
    Guid ReturnRequestId,
    string Reason
) : IRequest<bool>;
