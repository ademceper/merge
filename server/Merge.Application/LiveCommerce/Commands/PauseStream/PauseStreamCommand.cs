using MediatR;

namespace Merge.Application.LiveCommerce.Commands.PauseStream;

public record PauseStreamCommand(Guid StreamId) : IRequest<Unit>;
