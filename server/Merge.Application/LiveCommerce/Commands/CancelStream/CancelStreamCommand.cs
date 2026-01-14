using MediatR;

namespace Merge.Application.LiveCommerce.Commands.CancelStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CancelStreamCommand(Guid StreamId) : IRequest<Unit>;
