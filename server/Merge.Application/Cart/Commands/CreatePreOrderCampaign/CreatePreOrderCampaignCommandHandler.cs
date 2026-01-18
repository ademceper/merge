using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.CreatePreOrderCampaign;

public class CreatePreOrderCampaignCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreatePreOrderCampaignCommand, PreOrderCampaignDto>
{

    public async Task<PreOrderCampaignDto> Handle(CreatePreOrderCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = PreOrderCampaign.Create(
            request.Name,
            request.Description,
            request.ProductId,
            request.StartDate,
            request.EndDate,
            request.ExpectedDeliveryDate,
            request.MaxQuantity,
            request.DepositPercentage,
            request.SpecialPrice,
            request.NotifyOnAvailable);

        await context.Set<PreOrderCampaign>().AddAsync(campaign, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        campaign = await context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

        return mapper.Map<PreOrderCampaignDto>(campaign!);
    }
}

