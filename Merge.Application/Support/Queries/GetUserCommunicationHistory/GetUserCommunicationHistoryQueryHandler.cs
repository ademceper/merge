using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Queries.GetUserCommunicationHistory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserCommunicationHistoryQueryHandler : IRequestHandler<GetUserCommunicationHistoryQuery, CommunicationHistoryDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly SupportSettings _settings;

    public GetUserCommunicationHistoryQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _mapper = mapper;
        _settings = settings.Value;
    }

    public async Task<CommunicationHistoryDto> Handle(GetUserCommunicationHistoryQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        IQueryable<CustomerCommunication> query = _context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId);

        var totalCommunications = await query.CountAsync(cancellationToken);
        var communicationsByType = await query
            .GroupBy(c => c.CommunicationType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
        var communicationsByChannel = await query
            .GroupBy(c => c.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Channel, x => x.Count, cancellationToken);
        var lastCommunicationDate = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => (DateTime?)c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Get recent communications
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        var recent = await query
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .OrderByDescending(c => c.CreatedAt)
            // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
            .Take(_settings.DashboardRecentTicketsCount)
            .ToListAsync(cancellationToken);

        var history = new CommunicationHistoryDto(
            request.UserId,
            $"{user.FirstName} {user.LastName}",
            totalCommunications,
            communicationsByType,
            communicationsByChannel,
            _mapper.Map<List<CustomerCommunicationDto>>(recent),
            lastCommunicationDate
        );

        return history;
    }
}
