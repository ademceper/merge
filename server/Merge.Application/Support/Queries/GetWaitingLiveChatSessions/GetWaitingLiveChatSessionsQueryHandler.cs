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

namespace Merge.Application.Support.Queries.GetWaitingLiveChatSessions;

public class GetWaitingLiveChatSessionsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetWaitingLiveChatSessionsQuery, IEnumerable<LiveChatSessionDto>>
{

    public async Task<IEnumerable<LiveChatSessionDto>> Handle(GetWaitingLiveChatSessionsQuery request, CancellationToken cancellationToken)
    {
        // Not: Şu anda sadece 1 Include var ama gelecekte ek Include'lar eklenebilir
        var sessions = await context.Set<LiveChatSession>()
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => s.Status == ChatSessionStatus.Waiting)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }
}
