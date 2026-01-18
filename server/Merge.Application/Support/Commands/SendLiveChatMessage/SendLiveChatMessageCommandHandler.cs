using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Microsoft.Extensions.Options;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.SendLiveChatMessage;

public class SendLiveChatMessageCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<SendLiveChatMessageCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<SendLiveChatMessageCommand, LiveChatMessageDto>
{

    public async Task<LiveChatMessageDto> Handle(SendLiveChatMessageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending live chat message. SessionId: {SessionId}, SenderId: {SenderId}, MessageType: {MessageType}",
            request.SessionId, request.SenderId, request.MessageType);

        var session = await context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            logger.LogWarning("Live chat session {SessionId} not found while sending message", request.SessionId);
            throw new NotFoundException("Oturum", request.SessionId);
        }

        string senderType = "User";
        if (request.SenderId.HasValue)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.SenderId.Value, cancellationToken);

            if (user != null)
            {
                var isAgent = await context.UserRoles
                    .AsNoTracking()
                    .Where(ur => ur.UserId == user.Id)
                    .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .AnyAsync(r => r == "Admin" || r == "Manager" || r == "Support", cancellationToken);

                if (isAgent)
                {
                    senderType = "Agent";
                }
            }
        }

        var message = LiveChatMessage.Create(
            request.SessionId,
            session.SessionId,
            request.SenderId,
            senderType,
            request.Content,
            request.MessageType,
            request.FileUrl,
            request.FileName,
            request.IsInternal);

        await context.Set<LiveChatMessage>().AddAsync(message, cancellationToken);
        
        session.AddMessage(senderType);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Live chat message sent successfully. SessionId: {SessionId}, MessageId: {MessageId}, SenderType: {SenderType}",
            request.SessionId, message.Id, senderType);

        message = await context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

        return mapper.Map<LiveChatMessageDto>(message!);
    }
}
