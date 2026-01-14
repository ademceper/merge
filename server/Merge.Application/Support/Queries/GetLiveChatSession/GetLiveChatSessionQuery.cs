using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetLiveChatSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLiveChatSessionQuery(
    Guid SessionId
) : IRequest<LiveChatSessionDto?>;
