using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetAgentLiveChatSessions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAgentLiveChatSessionsQuery(
    Guid AgentId,
    string? Status = null
) : IRequest<IEnumerable<LiveChatSessionDto>>;
