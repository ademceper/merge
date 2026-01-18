using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetLiveChatSessionMessages;

public record GetLiveChatSessionMessagesQuery(
    Guid SessionId
) : IRequest<IEnumerable<LiveChatMessageDto>>;
