using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetUserLiveChatSessions;

public class GetUserLiveChatSessionsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetUserLiveChatSessionsQuery, IEnumerable<LiveChatSessionDto>>
{

    public async Task<IEnumerable<LiveChatSessionDto>> Handle(GetUserLiveChatSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await context.Set<LiveChatSession>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }
}
