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

namespace Merge.Application.Marketing.Commands.RecordEmailOpen;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class RecordEmailOpenCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RecordEmailOpenCommandHandler> logger) : IRequestHandler<RecordEmailOpenCommand, Unit>
{
    public async Task<Unit> Handle(RecordEmailOpenCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email açılması kaydediliyor. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            request.CampaignId, request.SubscriberId);

        var recipient = await context.Set<EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == request.CampaignId && r.SubscriberId == request.SubscriberId, cancellationToken);

        if (recipient == null)
        {
            logger.LogWarning("EmailCampaignRecipient not found. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
                request.CampaignId, request.SubscriberId);
            return Unit.Value;
        }

        var wasFirstOpen = recipient.OpenedAt == null;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        recipient.RecordEmailOpened();

        if (wasFirstOpen)
        {
            // ✅ PERFORMANCE: Batch load campaign and subscriber (N+1 fix)
            var campaign = await context.Set<EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

            if (campaign != null)
            {
                var newOpenedCount = campaign.OpenedCount + 1;
                var deliveredCount = campaign.DeliveredCount > 0 ? campaign.DeliveredCount : campaign.SentCount;
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                campaign.UpdateStatistics(
                    deliveredCount,
                    newOpenedCount,
                    campaign.ClickedCount,
                    campaign.BouncedCount,
                    campaign.UnsubscribedCount
                );
            }

            var subscriber = await context.Set<EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == request.SubscriberId, cancellationToken);

            if (subscriber != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                subscriber.RecordEmailOpened();
            }
        }
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email açılması kaydedildi. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, OpenCount: {OpenCount}",
            request.CampaignId, request.SubscriberId, recipient.OpenCount);

        return Unit.Value;
    }
}
