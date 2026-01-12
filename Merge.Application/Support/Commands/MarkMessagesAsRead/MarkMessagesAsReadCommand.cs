using MediatR;

namespace Merge.Application.Support.Commands.MarkMessagesAsRead;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MarkMessagesAsReadCommand(
    Guid SessionId,
    Guid UserId
) : IRequest<bool>;
