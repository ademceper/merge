using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.RecordEmailClick;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class RecordEmailClickCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RecordEmailClickCommandHandler> logger) : IRequestHandler<RecordEmailClickCommand, Unit>
{
    public async Task<Unit> Handle(RecordEmailClickCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email tıklaması kaydediliyor. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
            request.CampaignId, request.SubscriberId);

        var recipient = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaignRecipient>()
            .FirstOrDefaultAsync(r => r.CampaignId == request.CampaignId && r.SubscriberId == request.SubscriberId, cancellationToken);

        if (recipient == null)
        {
            logger.LogWarning("EmailCampaignRecipient not found. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}",
                request.CampaignId, request.SubscriberId);
            return Unit.Value;
        }

        var wasFirstClick = recipient.ClickedAt == null;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        recipient.RecordEmailClicked();

        if (wasFirstClick)
        {
            // ✅ PERFORMANCE: Batch load campaign and subscriber (N+1 fix)
            var campaign = await context.Set<Merge.Domain.Modules.Marketing.EmailCampaign>()
                .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

            if (campaign != null)
            {
                var newClickedCount = campaign.ClickedCount + 1;
                var deliveredCount = campaign.DeliveredCount > 0 ? campaign.DeliveredCount : campaign.SentCount;
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                campaign.UpdateStatistics(
                    deliveredCount,
                    campaign.OpenedCount,
                    newClickedCount,
                    campaign.BouncedCount,
                    campaign.UnsubscribedCount
                );
            }

            var subscriber = await context.Set<Merge.Domain.Modules.Marketing.EmailSubscriber>()
                .FirstOrDefaultAsync(s => s.Id == request.SubscriberId, cancellationToken);

            if (subscriber != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                subscriber.RecordEmailClicked();
            }
        }
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Email tıklaması kaydedildi. CampaignId: {CampaignId}, SubscriberId: {SubscriberId}, ClickCount: {ClickCount}",
            request.CampaignId, request.SubscriberId, recipient.ClickCount);

        return Unit.Value;
    }
}
