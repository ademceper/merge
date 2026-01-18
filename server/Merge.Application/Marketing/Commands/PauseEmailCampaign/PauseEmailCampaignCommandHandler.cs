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

namespace Merge.Application.Marketing.Commands.PauseEmailCampaign;

public class PauseEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<PauseEmailCampaignCommandHandler> logger) : IRequestHandler<PauseEmailCampaignCommand, bool>
{
    public async Task<bool> Handle(PauseEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Pausing email campaign. CampaignId: {CampaignId}", request.Id);

        var campaign = await context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign == null)
        {
            logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        campaign.Pause();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Email campaign paused successfully. CampaignId: {CampaignId}", request.Id);

        return true;
    }
}
