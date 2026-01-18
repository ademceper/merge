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

public class CancelEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<CancelEmailCampaignCommandHandler> logger) : IRequestHandler<CancelEmailCampaignCommand, bool>
{
    public async Task<bool> Handle(CancelEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Cancelling email campaign. CampaignId: {CampaignId}", request.Id);

        var campaign = await context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign is null)
        {
            logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        campaign.Cancel();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Email campaign cancelled successfully. CampaignId: {CampaignId}", request.Id);

        return true;
    }
}
