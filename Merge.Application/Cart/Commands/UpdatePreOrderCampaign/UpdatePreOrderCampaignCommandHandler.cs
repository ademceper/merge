using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;

namespace Merge.Application.Cart.Commands.UpdatePreOrderCampaign;

public class UpdatePreOrderCampaignCommandHandler : IRequestHandler<UpdatePreOrderCampaignCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePreOrderCampaignCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<Domain.Entities.PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign == null) return false;

        campaign.UpdateBasicInfo(request.Name, request.Description, request.MaxQuantity);
        campaign.UpdateDates(request.StartDate, request.EndDate, request.ExpectedDeliveryDate);
        campaign.UpdatePricing(request.DepositPercentage, request.SpecialPrice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

