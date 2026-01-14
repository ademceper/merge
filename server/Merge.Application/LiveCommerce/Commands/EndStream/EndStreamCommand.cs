using MediatR;

namespace Merge.Application.LiveCommerce.Commands.EndStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EndStreamCommand(Guid StreamId) : IRequest<Unit>;

