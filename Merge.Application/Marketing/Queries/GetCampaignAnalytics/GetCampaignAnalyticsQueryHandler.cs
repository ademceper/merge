using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetCampaignAnalyticsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCampaignAnalyticsQuery, EmailCampaignAnalyticsDto?>
{
    public async Task<EmailCampaignAnalyticsDto?> Handle(GetCampaignAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign == null)
        {
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = mapper.Map<EmailCampaignAnalyticsDto>(campaign);
        
        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece property assignment)
        var bounceRate = campaign.SentCount > 0 ? (decimal)campaign.BouncedCount / campaign.SentCount * 100 : 0;
        var unsubscribeRate = campaign.SentCount > 0 ? (decimal)campaign.UnsubscribedCount / campaign.SentCount * 100 : 0;
        
        return dto with { BounceRate = bounceRate, UnsubscribeRate = unsubscribeRate };
    }
}
