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

public class GetLoyaltyTiersQueryHandler : IRequestHandler<GetLoyaltyTiersQuery, IEnumerable<LoyaltyTierDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetLoyaltyTiersQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LoyaltyTierDto>> Handle(GetLoyaltyTiersQuery request, CancellationToken cancellationToken)
    {
        var tiers = await _context.Set<LoyaltyTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Level)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<LoyaltyTierDto>>(tiers);
    }
}
