using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.PatchPreOrderCampaign;

/// <summary>
/// Handler for PatchPreOrderCampaignCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchPreOrderCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork) : IRequestHandler<PatchPreOrderCampaignCommand, bool>
{
    public async Task<bool> Handle(PatchPreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await context.Set<PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        if (campaign is null) return false;

        // Apply partial updates - get existing values if not provided
        var name = request.PatchDto.Name ?? campaign.Name;
        var description = request.PatchDto.Description ?? campaign.Description;
        var startDate = request.PatchDto.StartDate ?? campaign.StartDate;
        var endDate = request.PatchDto.EndDate ?? campaign.EndDate;
        var expectedDeliveryDate = request.PatchDto.ExpectedDeliveryDate ?? campaign.ExpectedDeliveryDate;
        var maxQuantity = request.PatchDto.MaxQuantity ?? campaign.MaxQuantity;
        var depositPercentage = request.PatchDto.DepositPercentage ?? campaign.DepositPercentage;
        var specialPrice = request.PatchDto.SpecialPrice ?? campaign.SpecialPrice;

        campaign.UpdateBasicInfo(name, description, maxQuantity);
        campaign.UpdateDates(startDate, endDate, expectedDeliveryDate);
        campaign.UpdatePricing(depositPercentage, specialPrice);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
