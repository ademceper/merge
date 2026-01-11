using MediatR;

namespace Merge.Application.Order.Commands.CompleteOrderSplit;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CompleteOrderSplitCommand(
    Guid SplitId
) : IRequest<bool>;
