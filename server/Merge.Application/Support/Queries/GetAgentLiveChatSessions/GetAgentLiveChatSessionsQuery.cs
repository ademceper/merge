using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetAgentLiveChatSessions;

public record GetAgentLiveChatSessionsQuery(
    Guid AgentId,
    string? Status = null
) : IRequest<IEnumerable<LiveChatSessionDto>>;
