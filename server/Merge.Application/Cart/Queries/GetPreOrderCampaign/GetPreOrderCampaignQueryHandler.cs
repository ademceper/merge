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

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

public class GetPreOrderCampaignQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetPreOrderCampaignQuery, PreOrderCampaignDto?>
{

    public async Task<PreOrderCampaignDto?> Handle(GetPreOrderCampaignQuery request, CancellationToken cancellationToken)
    {

        var campaign = await context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        return campaign is not null ? mapper.Map<PreOrderCampaignDto>(campaign) : null;
    }
}

