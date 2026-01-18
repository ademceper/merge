using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetAgentLiveChatSessions;

public class GetAgentLiveChatSessionsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetAgentLiveChatSessionsQuery, IEnumerable<LiveChatSessionDto>>
{

    public async Task<IEnumerable<LiveChatSessionDto>> Handle(GetAgentLiveChatSessionsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<LiveChatSession> query = context.Set<LiveChatSession>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.AgentId == request.AgentId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<ChatSessionStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(s => s.Status == statusEnum);
            }
        }

        var sessions = await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }
}
