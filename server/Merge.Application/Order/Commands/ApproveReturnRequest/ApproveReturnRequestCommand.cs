using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.ApproveReturnRequest;

public record ApproveReturnRequestCommand(
    Guid ReturnRequestId
) : IRequest<bool>;
