using MediatR;

namespace Merge.Application.LiveCommerce.Commands.DeleteLiveStream;

public record DeleteLiveStreamCommand(Guid StreamId) : IRequest<Unit>;
