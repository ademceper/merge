using MediatR;

namespace Merge.Application.LiveCommerce.Commands.DeleteLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteLiveStreamCommand(Guid StreamId) : IRequest<Unit>;

