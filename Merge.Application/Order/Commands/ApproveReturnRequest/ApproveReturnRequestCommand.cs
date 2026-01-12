using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.ApproveReturnRequest;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApproveReturnRequestCommand(
    Guid ReturnRequestId
) : IRequest<bool>;
