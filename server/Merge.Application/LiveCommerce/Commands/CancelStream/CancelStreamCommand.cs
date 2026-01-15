using MediatR;

namespace Merge.Application.LiveCommerce.Commands.CancelStream;

public record CancelStreamCommand(Guid StreamId) : IRequest<Unit>;
