using MediatR;
using Microsoft.EntityFrameworkCore;
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

namespace Merge.Application.Support.Queries.GetLiveChatSessionBySessionId;

public class GetLiveChatSessionBySessionIdQueryHandler(IDbContext context, IMapper mapper, IOptions<SupportSettings> settings) : IRequestHandler<GetLiveChatSessionBySessionIdQuery, LiveChatSessionDto?>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<LiveChatSessionDto?> Handle(GetLiveChatSessionBySessionIdQuery request, CancellationToken cancellationToken)
    {
        var session = await context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(supportConfig.MaxRecentChatMessages))
            .FirstOrDefaultAsync(s => s.SessionId == request.SessionId, cancellationToken);

        if (session is null) return null;

        var dto = mapper.Map<LiveChatSessionDto>(session);

        if (session.Messages is null || session.Messages.Count == 0)
        {
            var recentMessages = await context.Set<LiveChatMessage>()
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.SessionId == session.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(supportConfig.MaxRecentChatMessages)
                .ToListAsync(cancellationToken);
            dto.RecentMessages = mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }
        else
        {
            var recentMessages = session.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Take(supportConfig.MaxRecentChatMessages)
                .ToList();
            dto.RecentMessages = mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }

        return dto;
    }
}
