using MediatR;

namespace Merge.Application.Logistics.Commands.MarkPickPackAsShipped;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkPickPackAsShippedCommand(Guid PickPackId) : IRequest<Unit>;

