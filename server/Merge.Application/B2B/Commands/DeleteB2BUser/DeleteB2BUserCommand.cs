using MediatR;

namespace Merge.Application.B2B.Commands.DeleteB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteB2BUserCommand(Guid Id) : IRequest<bool>;

