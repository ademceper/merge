using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.MarkMessagesAsRead;

public class MarkMessagesAsReadCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkMessagesAsReadCommandHandler> logger) : IRequestHandler<MarkMessagesAsReadCommand, bool>
{

    public async Task<bool> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking messages as read for session {SessionId} by user {UserId}", 
            request.SessionId, request.UserId);

        var session = await context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            logger.LogWarning("Live chat session {SessionId} not found for marking messages as read", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        var messages = await context.Set<LiveChatMessage>()
            .Where(m => m.SessionId == request.SessionId && !m.IsRead && m.SenderId != request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.MarkAsRead();
        }

        session.MarkMessagesAsRead();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Marked {MessageCount} messages as read for session {SessionId}", 
            messages.Count, request.SessionId);

        return true;
    }
}
