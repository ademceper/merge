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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAgentLiveChatSessionsQueryHandler : IRequestHandler<GetAgentLiveChatSessionsQuery, IEnumerable<LiveChatSessionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetAgentLiveChatSessionsQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LiveChatSessionDto>> Handle(GetAgentLiveChatSessionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<LiveChatSession> query = _context.Set<LiveChatSession>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Where(s => s.AgentId == request.AgentId);

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
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

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }
}
