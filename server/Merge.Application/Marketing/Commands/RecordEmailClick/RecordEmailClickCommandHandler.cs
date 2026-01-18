using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using EmailCampaignRecipient = Merge.Domain.Modules.Marketing.EmailCampaignRecipient;
using EmailCampaign = Merge.Domain.Modules.Marketing.EmailCampaign;
using EmailSubscriber = Merge.Domain.Modules.Marketing.EmailSubscriber;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.RecordEmailClick;

public class RecordEmailClickCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RecordEmailClickCommandHandler> logger) : IRequestHandler<RecordEmailClickCommand, Unit>
{
    public async Task<Unit> Handle(RecordEmailClickCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email tıklaması kaydediliyor. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            request.CampaignId, request.SubscriberId);

        var recipient = await context.Set<EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == request.CampaignId && r.SubscriberId == request.SubscriberId, cancellationToken);

        if (recipient == null)
        {
            logger.LogWarning("EmailCampaignRecipient not found. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
                request.CampaignId, request.SubscriberId);
            return Unit.Value;
        }

        var wasFirstClick = recipient.ClickedAt == null;

        recipient.RecordEmailClicked();

        if (wasFirstClick)
        {
            var campaign = await context.Set<EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

            if (campaign != null)
            {
                var newClickedCount = campaign.ClickedCount + 1;
                var deliveredCount = campaign.DeliveredCount > 0 ? campaign.DeliveredCount : campaign.SentCount;
                campaign.UpdateStatistics(
                    deliveredCount,
                    campaign.OpenedCount,
                    newClickedCount,
                    campaign.BouncedCount,
                    campaign.UnsubscribedCount
                );
            }

            var subscriber = await context.Set<EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == request.SubscriberId, cancellationToken);

            if (subscriber != null)
            {
                subscriber.RecordEmailClicked();
            }
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Email tıklaması kaydedildi. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, ClickCount: {ClickCount}",
            request.CampaignId, request.SubscriberId, recipient.ClickCount);

        return Unit.Value;
    }
}
