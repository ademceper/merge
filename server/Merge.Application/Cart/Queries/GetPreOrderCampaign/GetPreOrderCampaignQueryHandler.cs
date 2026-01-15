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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPreOrderCampaignQueryHandler(
    IDbContext context,
    IMapper mapper) : IRequestHandler<GetPreOrderCampaignQuery, PreOrderCampaignDto?>
{

    public async Task<PreOrderCampaignDto?> Handle(GetPreOrderCampaignQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // Note: Single Include, AsSplitQuery not strictly necessary but good practice
        var campaign = await context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        return campaign is not null ? mapper.Map<PreOrderCampaignDto>(campaign) : null;
    }
}

