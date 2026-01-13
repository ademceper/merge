using MediatR;

namespace Merge.Application.LiveCommerce.Commands.PauseStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record PauseStreamCommand(Guid StreamId) : IRequest<Unit>;
