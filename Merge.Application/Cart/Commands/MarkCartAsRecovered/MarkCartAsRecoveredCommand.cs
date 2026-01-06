using MediatR;

namespace Merge.Application.Cart.Commands.MarkCartAsRecovered;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkCartAsRecoveredCommand(Guid CartId) : IRequest;

