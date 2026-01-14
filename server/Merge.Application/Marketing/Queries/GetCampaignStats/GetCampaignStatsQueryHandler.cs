using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetCampaignStatsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCampaignStatsQuery, EmailCampaignStatsDto>
{
    public async Task<EmailCampaignStatsDto> Handle(GetCampaignStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var totalCampaigns = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeCampaigns = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
            .AsNoTracking()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sending || c.Status == EmailCampaignStatus.Scheduled, cancellationToken);

        var totalSubscribers = await context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeSubscribers = await context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
            .AsNoTracking()
            .CountAsync(s => s.IsSubscribed, cancellationToken);

        var totalEmailsSent = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
            .AsNoTracking()
            .SumAsync(c => (long)c.SentCount, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var sentCampaignsCount = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
            .AsNoTracking()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sent, cancellationToken);

        var avgOpenRate = sentCampaignsCount > 0
            ? await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
                .AsNoTracking()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.OpenRate, cancellationToken) ?? 0
            : 0;

        var avgClickRate = sentCampaignsCount > 0
            ? await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
                .AsNoTracking()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.ClickRate, cancellationToken) ?? 0
            : 0;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var recentCampaigns = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
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
            mapper.Map<List<EmailCampaignDto>>(recentCampaigns));
    }
}
