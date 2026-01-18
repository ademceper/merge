using MediatR;

namespace Merge.Application.Support.Commands.CloseLiveChatSession;

public record CloseLiveChatSessionCommand(
    Guid SessionId
) : IRequest<bool>;
