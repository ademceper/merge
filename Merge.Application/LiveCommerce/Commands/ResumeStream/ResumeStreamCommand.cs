using MediatR;

namespace Merge.Application.LiveCommerce.Commands.ResumeStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ResumeStreamCommand(Guid StreamId) : IRequest<Unit>;
