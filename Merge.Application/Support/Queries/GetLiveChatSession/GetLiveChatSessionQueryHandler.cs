using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Queries.GetLiveChatSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetLiveChatSessionQueryHandler : IRequestHandler<GetLiveChatSessionQuery, LiveChatSessionDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly SupportSettings _settings;

    public GetLiveChatSessionQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _mapper = mapper;
        _settings = settings.Value;
    }

    public async Task<LiveChatSessionDto?> Handle(GetLiveChatSessionQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        var session = await _context.Set<LiveChatSession>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.User)
            .Include(s => s.Agent)
            .Include(s => s.Messages.OrderByDescending(m => m.CreatedAt).Take(_settings.MaxRecentChatMessages))
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null) return null;

        var dto = _mapper.Map<LiveChatSessionDto>(session);

        // ✅ PERFORMANCE: Batch load recent messages if not already loaded
        if (session.Messages == null || session.Messages.Count == 0)
        {
            var recentMessages = await _context.Set<LiveChatMessage>()
                .AsNoTracking()
                .Include(m => m.Sender)
                .Where(m => m.SessionId == session.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(_settings.MaxRecentChatMessages)
                .ToListAsync(cancellationToken);
            dto.RecentMessages = _mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }
        else
        {
            var recentMessages = session.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Take(_settings.MaxRecentChatMessages)
                .ToList();
            dto.RecentMessages = _mapper.Map<List<LiveChatMessageDto>>(recentMessages);
        }

        return dto;
    }
}
