using MediatR;

namespace Merge.Application.Logistics.Commands.StartPacking;

public record StartPackingCommand(
    Guid PickPackId,
    Guid UserId) : IRequest<Unit>;

