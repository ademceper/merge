using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCampaignStatsQueryHandler : IRequestHandler<GetCampaignStatsQuery, EmailCampaignStatsDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetCampaignStatsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmailCampaignStatsDto> Handle(GetCampaignStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var totalCampaigns = await _context.Set<Merge.Domain.Entities.EmailCampaign>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeCampaigns = await _context.Set<Merge.Domain.Entities.EmailCampaign>()
            .AsNoTracking()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sending || c.Status == EmailCampaignStatus.Scheduled, cancellationToken);

        var totalSubscribers = await _context.Set<Merge.Domain.Entities.EmailSubscriber>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeSubscribers = await _context.Set<Merge.Domain.Entities.EmailSubscriber>()
            .AsNoTracking()
            .CountAsync(s => s.IsSubscribed, cancellationToken);

        var totalEmailsSent = await _context.Set<Merge.Domain.Entities.EmailCampaign>()
            .AsNoTracking()
            .SumAsync(c => (long)c.SentCount, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var sentCampaignsCount = await _context.Set<Merge.Domain.Entities.EmailCampaign>()
            .AsNoTracking()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sent, cancellationToken);

        var avgOpenRate = sentCampaignsCount > 0
            ? await _context.Set<Merge.Domain.Entities.EmailCampaign>()
                .AsNoTracking()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.OpenRate, cancellationToken) ?? 0
            : 0;

        var avgClickRate = sentCampaignsCount > 0
            ? await _context.Set<Merge.Domain.Entities.EmailCampaign>()
                .AsNoTracking()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.ClickRate, cancellationToken) ?? 0
            : 0;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var recentCampaigns = await _context.Set<Merge.Domain.Entities.EmailCampaign>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return new EmailCampaignStatsDto(
            totalCampaigns,
            activeCampaigns,
            totalSubscribers,
            activeSubscribers,
            totalEmailsSent,
            avgOpenRate,
            avgClickRate,
            _mapper.Map<List<EmailCampaignDto>>(recentCampaigns));
    }
}
