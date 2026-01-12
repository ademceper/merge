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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllCommissionTiersQueryHandler : IRequestHandler<GetAllCommissionTiersQuery, IEnumerable<CommissionTierDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCommissionTiersQueryHandler> _logger;

    public GetAllCommissionTiersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllCommissionTiersQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CommissionTierDto>> Handle(GetAllCommissionTiersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting all commission tiers");

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var tiers = await _context.Set<CommissionTier>()
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Priority)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<CommissionTierDto>>(tiers);
    }
}
