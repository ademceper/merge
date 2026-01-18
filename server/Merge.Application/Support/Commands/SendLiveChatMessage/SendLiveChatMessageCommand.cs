using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Commands.SendLiveChatMessage;

public record SendLiveChatMessageCommand(
    Guid SessionId,
    Guid? SenderId,
    string Content,
    string MessageType = "Text",
    string? FileUrl = null,
    string? FileName = null,
    bool IsInternal = false
) : IRequest<LiveChatMessageDto>;
