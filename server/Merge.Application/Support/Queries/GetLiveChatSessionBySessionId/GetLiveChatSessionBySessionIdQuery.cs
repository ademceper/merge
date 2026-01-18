using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetLiveChatSessionBySessionId;

public record GetLiveChatSessionBySessionIdQuery(
    string SessionId
) : IRequest<LiveChatSessionDto?>;
