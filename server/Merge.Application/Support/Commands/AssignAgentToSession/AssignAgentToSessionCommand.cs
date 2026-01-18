using MediatR;

namespace Merge.Application.Support.Commands.AssignAgentToSession;

public record AssignAgentToSessionCommand(
    Guid SessionId,
    Guid AgentId
) : IRequest<bool>;
