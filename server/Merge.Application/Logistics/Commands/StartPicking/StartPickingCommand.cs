using MediatR;

namespace Merge.Application.Logistics.Commands.StartPicking;

public record StartPickingCommand(
    Guid PickPackId,
    Guid UserId) : IRequest<Unit>;

