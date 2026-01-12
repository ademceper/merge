using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Queries.GetLiveChatSessionMessages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetLiveChatSessionMessagesQuery(
    Guid SessionId
) : IRequest<IEnumerable<LiveChatMessageDto>>;
