using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetLiveChatSession;

public record GetLiveChatSessionQuery(
    Guid SessionId
) : IRequest<LiveChatSessionDto?>;
