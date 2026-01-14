using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetUserLiveChatSessions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserLiveChatSessionsQuery(
    Guid UserId
) : IRequest<IEnumerable<LiveChatSessionDto>>;
