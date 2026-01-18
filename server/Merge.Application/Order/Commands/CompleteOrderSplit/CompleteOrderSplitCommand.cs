using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CompleteOrderSplit;

public record CompleteOrderSplitCommand(
    Guid SplitId
) : IRequest<bool>;
