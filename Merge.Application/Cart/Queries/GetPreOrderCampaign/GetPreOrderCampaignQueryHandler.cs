using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.Cart.Queries.GetPreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPreOrderCampaignQueryHandler : IRequestHandler<GetPreOrderCampaignQuery, PreOrderCampaignDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetPreOrderCampaignQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PreOrderCampaignDto?> Handle(GetPreOrderCampaignQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        return campaign != null ? _mapper.Map<PreOrderCampaignDto>(campaign) : null;
    }
}

