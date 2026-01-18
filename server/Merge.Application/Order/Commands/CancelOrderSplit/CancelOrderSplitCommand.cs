using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CancelOrderSplit;

public record CancelOrderSplitCommand(
    Guid SplitId
) : IRequest<bool>;
