using MediatR;

namespace Merge.Application.Support.Commands.AssignAgentToSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AssignAgentToSessionCommand(
    Guid SessionId,
    Guid AgentId
) : IRequest<bool>;
