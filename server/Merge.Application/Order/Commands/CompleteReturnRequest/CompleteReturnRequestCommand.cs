using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CompleteReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CompleteReturnRequestCommand(
    Guid ReturnRequestId,
    string TrackingNumber
) : IRequest<bool>;
