using MediatR;

namespace Merge.Application.Logistics.Commands.MarkPickPackAsShipped;

public record MarkPickPackAsShippedCommand(Guid PickPackId) : IRequest<Unit>;

