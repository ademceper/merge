using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Commands.DeleteEmailCampaign;

public class DeleteEmailCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEmailCampaignCommandHandler> logger) : IRequestHandler<DeleteEmailCampaignCommand, bool>
{
    public async Task<bool> Handle(DeleteEmailCampaignCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting email campaign. CampaignId: {CampaignId}", request.Id);

        var campaign = await context.Set<EmailCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (campaign == null)
        {
            logger.LogWarning("Email campaign not found. CampaignId: {CampaignId}", request.Id);
            return false;
        }

        if (campaign.Status == Merge.Domain.Enums.EmailCampaignStatus.Sending)
        {
            logger.LogWarning("Cannot delete campaign that is currently sending. CampaignId: {CampaignId}", request.Id);
            throw new BusinessException("Şu anda gönderilmekte olan bir kampanya silinemez.");
        }

        campaign.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Email campaign deleted successfully. CampaignId: {CampaignId}", request.Id);

        return true;
    }
}
