using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Queries.GetWaitingLiveChatSessions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetWaitingLiveChatSessionsQueryHandler : IRequestHandler<GetWaitingLiveChatSessionsQuery, IEnumerable<LiveChatSessionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetWaitingLiveChatSessionsQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LiveChatSessionDto>> Handle(GetWaitingLiveChatSessionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        // Not: Şu anda sadece 1 Include var ama gelecekte ek Include'lar eklenebilir
        var sessions = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.User)
            .Where(s => s.Status == ChatSessionStatus.Waiting)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatSessionDto>>(sessions);
    }
}
