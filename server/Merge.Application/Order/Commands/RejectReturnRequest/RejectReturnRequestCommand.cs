using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.RejectReturnRequest;

public record RejectReturnRequestCommand(
    Guid ReturnRequestId,
    string Reason
) : IRequest<bool>;
