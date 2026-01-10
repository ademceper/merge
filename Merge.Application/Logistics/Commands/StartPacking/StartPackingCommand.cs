using MediatR;

namespace Merge.Application.Logistics.Commands.StartPacking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record StartPackingCommand(
    Guid PickPackId,
    Guid UserId) : IRequest<Unit>;

