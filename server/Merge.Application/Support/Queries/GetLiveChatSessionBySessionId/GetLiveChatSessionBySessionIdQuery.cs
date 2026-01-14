using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetLiveChatSessionBySessionId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLiveChatSessionBySessionIdQuery(
    string SessionId
) : IRequest<LiveChatSessionDto?>;
