using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetWaitingLiveChatSessions;

public record GetWaitingLiveChatSessionsQuery() : IRequest<IEnumerable<LiveChatSessionDto>>;
