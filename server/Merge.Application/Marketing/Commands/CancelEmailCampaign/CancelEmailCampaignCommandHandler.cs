using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.CancelEmailCampaign;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CancelEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CancelEmailCampaignCommandHandler> logger) : IRequestHandler<CancelEmailCampaignCommand, bool>
{
    public async Task<bool> Handle(CancelEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Cancelling email campaign. CampaignId: {CampaignId}", request.Id);

        // ✅ PERFORMANCE: Removed manual !c.IsDeleted (Global Query Filter)
        var campaign = await context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign == null)
        {
            logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        campaign.Cancel();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Email campaign cancelled successfully. CampaignId: {CampaignId}", request.Id);

        return true;
    }
}
