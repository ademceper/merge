using MediatR;

namespace Merge.Application.LiveCommerce.Commands.StartStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record StartStreamCommand(Guid StreamId) : IRequest<Unit>;

