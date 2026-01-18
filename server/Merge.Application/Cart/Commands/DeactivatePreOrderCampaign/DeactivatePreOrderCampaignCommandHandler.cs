using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using PreOrderCampaign = Merge.Domain.Modules.Marketing.PreOrderCampaign;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

public class DeactivatePreOrderCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivatePreOrderCampaignCommand, bool>
{

    public async Task<bool> Handle(DeactivatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign is null) return false;

        campaign.Deactivate();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

