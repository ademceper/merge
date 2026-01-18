using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using EmailCampaign = Merge.Domain.Modules.Marketing.EmailCampaign;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

public class GetCampaignAnalyticsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCampaignAnalyticsQuery, EmailCampaignAnalyticsDto?>
{
    public async Task<EmailCampaignAnalyticsDto?> Handle(GetCampaignAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<EmailCampaign>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign is null)
        {
            return null;
        }

        var dto = mapper.Map<EmailCampaignAnalyticsDto>(campaign);
        
        var bounceRate = campaign.SentCount > 0 ? (decimal)campaign.BouncedCount / campaign.SentCount * 100 : 0;
        var unsubscribeRate = campaign.SentCount > 0 ? (decimal)campaign.UnsubscribedCount / campaign.SentCount * 100 : 0;
        
        return dto with { BounceRate = bounceRate, UnsubscribeRate = unsubscribeRate };
    }
}
