using MediatR;

namespace Merge.Application.Logistics.Commands.CompletePicking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CompletePickingCommand(Guid PickPackId) : IRequest<Unit>;

