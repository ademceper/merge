using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

public class DeactivatePreOrderCampaignCommandHandler : IRequestHandler<DeactivatePreOrderCampaignCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePreOrderCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeactivatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<Domain.Entities.PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign == null) return false;

        campaign.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

