using MediatR;

namespace Merge.Application.LiveCommerce.Commands.StartStream;

public record StartStreamCommand(Guid StreamId) : IRequest<Unit>;
