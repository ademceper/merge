using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.ScheduleEmailCampaign;

public class ScheduleEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ScheduleEmailCampaignCommandHandler> logger) : IRequestHandler<ScheduleEmailCampaignCommand, bool>
{
    public async Task<bool> Handle(ScheduleEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Scheduling email campaign. CampaignId: {CampaignId}, ScheduledAt: {ScheduledAt}", 
            request.Id, request.ScheduledAt);

        var campaign = await context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign is null)
        {
            logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        campaign.Schedule(request.ScheduledAt);

        // Prepare recipients (EmailCampaignService'deki PrepareCampaignRecipientsAsync mantığı)
        // Bu mantık domain'e taşınabilir veya burada kalabilir
        var subscribers = await GetTargetedSubscribersAsync(campaign.TargetSegment, cancellationToken);
        campaign.StartSending(subscribers.Count);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Email campaign scheduled successfully. CampaignId: {CampaignId}, ScheduledAt: {ScheduledAt}, Recipients: {Recipients}", 
            request.Id, request.ScheduledAt, subscribers.Count);

        return true;
    }

    private async Task<List<EmailSubscriber>> GetTargetedSubscribersAsync(string segment, CancellationToken cancellationToken)
    {
        IQueryable<EmailSubscriber> query = context.Set<EmailSubscriber>()
            .Where(s => s.IsSubscribed);

        switch (segment.ToLower())
        {
            case "all":
                break;
            case "active":
                query = query.Where(s => s.EmailsOpened > 0 || s.EmailsClicked > 0);
                break;
            case "inactive":
                query = query.Where(s => s.EmailsOpened == 0 && s.EmailsClicked == 0);
                break;
        }

        return await query.ToListAsync(cancellationToken);
    }
}
