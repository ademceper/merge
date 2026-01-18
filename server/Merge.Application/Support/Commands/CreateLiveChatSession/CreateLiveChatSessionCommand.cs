using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Support.Commands.CreateLiveChatSession;

public record CreateLiveChatSessionCommand(
    Guid? UserId = null,
    string? GuestName = null,
    string? GuestEmail = null,
    string? Department = null
) : IRequest<LiveChatSessionDto>;
