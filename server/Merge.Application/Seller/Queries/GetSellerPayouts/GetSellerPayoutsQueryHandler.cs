using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerPayouts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerPayoutsQueryHandler : IRequestHandler<GetSellerPayoutsQuery, IEnumerable<CommissionPayoutDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerPayoutsQueryHandler> _logger;

    public GetSellerPayoutsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerPayoutsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<CommissionPayoutDto>> Handle(GetSellerPayoutsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller payouts. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes with nested ThenInclude)
        var payouts = await _context.Set<CommissionPayout>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Seller)
            .Include(p => p.Items)
                .ThenInclude(i => i.Commission)
                    .ThenInclude(c => c.Order)
            .Where(p => p.SellerId == request.SellerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<CommissionPayoutDto>>(payouts);
    }
}
