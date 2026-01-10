using MediatR;

namespace Merge.Application.Logistics.Commands.StartPicking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record StartPickingCommand(
    Guid PickPackId,
    Guid UserId) : IRequest<Unit>;

