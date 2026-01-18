using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTiers;

public class GetLoyaltyTiersQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetLoyaltyTiersQuery, IEnumerable<LoyaltyTierDto>>
{
    public async Task<IEnumerable<LoyaltyTierDto>> Handle(GetLoyaltyTiersQuery request, CancellationToken cancellationToken)
    {
        var tiers = await context.Set<LoyaltyTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Level)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LoyaltyTierDto>>(tiers);
    }
}
