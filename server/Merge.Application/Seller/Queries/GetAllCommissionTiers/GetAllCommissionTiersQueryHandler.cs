using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetAllCommissionTiers;

public class GetAllCommissionTiersQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllCommissionTiersQueryHandler> logger) : IRequestHandler<GetAllCommissionTiersQuery, IEnumerable<CommissionTierDto>>
{

    public async Task<IEnumerable<CommissionTierDto>> Handle(GetAllCommissionTiersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all commission tiers");

        var tiers = await context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<CommissionTierDto>>(tiers);
    }
}
