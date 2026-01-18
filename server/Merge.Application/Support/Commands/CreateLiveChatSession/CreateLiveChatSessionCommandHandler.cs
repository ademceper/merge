using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.CreateLiveChatSession;

public class CreateLiveChatSessionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateLiveChatSessionCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<CreateLiveChatSessionCommand, LiveChatSessionDto>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<LiveChatSessionDto> Handle(CreateLiveChatSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating live chat session. UserId: {UserId}, GuestName: {GuestName}, Department: {Department}",
            request.UserId, request.GuestName, request.Department);

        var sessionId = GenerateSessionId();

        var session = LiveChatSession.Create(
            sessionId,
            request.UserId,
            request.GuestName,
            request.GuestEmail,
            request.Department,
            null, // IP address will be set by controller if needed
            null); // User agent will be set by controller if needed

        await context.Set<LiveChatSession>().AddAsync(session, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Live chat session {SessionId} created successfully", sessionId);

        session = await context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(supportConfig.MaxRecentChatMessages))
            .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

        return mapper.Map<LiveChatSessionDto>(session!);
    }

    private string GenerateSessionId()
    {
        var datePart = DateTime.UtcNow.ToString(supportConfig.ChatSessionIdDateFormat);
        var guidPart = Guid.NewGuid().ToString().Substring(0, supportConfig.ChatSessionIdGuidLength).ToUpper();
        return $"{supportConfig.ChatSessionIdPrefix}{datePart}-{guidPart}";
    }
}
