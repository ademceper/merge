using MediatR;

namespace Merge.Application.LiveCommerce.Commands.EndStream;

public record EndStreamCommand(Guid StreamId) : IRequest<Unit>;
