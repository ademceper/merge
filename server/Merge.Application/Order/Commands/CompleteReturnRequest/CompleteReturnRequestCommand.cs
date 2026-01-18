using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CompleteReturnRequest;

public record CompleteReturnRequestCommand(
    Guid ReturnRequestId,
    string TrackingNumber
) : IRequest<bool>;
