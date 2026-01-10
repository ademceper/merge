using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;

namespace Merge.Application.Marketing.Queries.GetCampaignAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCampaignAnalyticsQueryHandler : IRequestHandler<GetCampaignAnalyticsQuery, EmailCampaignAnalyticsDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetCampaignAnalyticsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EmailCampaignAnalyticsDto?> Handle(GetCampaignAnalyticsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await _context.Set<Merge.Domain.Entities.EmailCampaign>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign == null)
        {
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<EmailCampaignAnalyticsDto>(campaign);
        
        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece property assignment)
        var bounceRate = campaign.SentCount > 0 ? (decimal)campaign.BouncedCount / campaign.SentCount * 100 : 0;
        var unsubscribeRate = campaign.SentCount > 0 ? (decimal)campaign.UnsubscribedCount / campaign.SentCount * 100 : 0;
        
        return dto with { BounceRate = bounceRate, UnsubscribeRate = unsubscribeRate };
    }
}
