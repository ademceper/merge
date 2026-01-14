using MediatR;

namespace Merge.Application.Support.Commands.CloseLiveChatSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CloseLiveChatSessionCommand(
    Guid SessionId
) : IRequest<bool>;
