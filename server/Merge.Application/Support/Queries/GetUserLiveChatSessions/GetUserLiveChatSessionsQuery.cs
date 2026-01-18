using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetUserLiveChatSessions;

public record GetUserLiveChatSessionsQuery(
    Guid UserId
) : IRequest<IEnumerable<LiveChatSessionDto>>;
