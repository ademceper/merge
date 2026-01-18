using MediatR;

namespace Merge.Application.Logistics.Commands.CompletePicking;

public record CompletePickingCommand(Guid PickPackId) : IRequest<Unit>;

