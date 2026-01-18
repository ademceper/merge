using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using EmailCampaign = Merge.Domain.Modules.Marketing.EmailCampaign;
using EmailSubscriber = Merge.Domain.Modules.Marketing.EmailSubscriber;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetCampaignStats;

public class GetCampaignStatsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCampaignStatsQuery, EmailCampaignStatsDto>
{
    public async Task<EmailCampaignStatsDto> Handle(GetCampaignStatsQuery request, CancellationToken cancellationToken)
    {
        var totalCampaigns = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeCampaigns = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sending || c.Status == EmailCampaignStatus.Scheduled, cancellationToken);

        var totalSubscribers = await context.Set<EmailSubscriber>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeSubscribers = await context.Set<EmailSubscriber>()
            .AsNoTracking()
            .CountAsync(s => s.IsSubscribed, cancellationToken);

        var totalEmailsSent = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .SumAsync(c => (long)c.SentCount, cancellationToken);

        var sentCampaignsCount = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .CountAsync(c => c.Status == EmailCampaignStatus.Sent, cancellationToken);

        var avgOpenRate = sentCampaignsCount > 0
            ? await context.Set<EmailCampaign>()
                .AsNoTracking()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.OpenRate, cancellationToken) ?? 0
            : 0;

        var avgClickRate = sentCampaignsCount > 0
            ? await context.Set<EmailCampaign>()
                .AsNoTracking()
                .Where(c => c.Status == EmailCampaignStatus.Sent)
                .AverageAsync(c => (decimal?)c.ClickRate, cancellationToken) ?? 0
            : 0;

        var recentCampaigns = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

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
