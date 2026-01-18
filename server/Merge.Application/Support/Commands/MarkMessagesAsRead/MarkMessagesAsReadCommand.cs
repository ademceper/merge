using MediatR;

namespace Merge.Application.Support.Commands.MarkMessagesAsRead;

public record MarkMessagesAsReadCommand(
    Guid SessionId,
    Guid UserId
) : IRequest<bool>;
