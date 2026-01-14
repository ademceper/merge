using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetSellerBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerBadgesQueryHandler : IRequestHandler<GetSellerBadgesQuery, IEnumerable<SellerTrustBadgeDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerBadgesQueryHandler> _logger;

    public GetSellerBadgesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerBadgesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SellerTrustBadgeDto>> Handle(GetSellerBadgesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching seller badges. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !stb.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var badges = await _context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .Where(stb => stb.SellerId == request.SellerId && stb.IsActive)
            .OrderBy(stb => stb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SellerTrustBadgeDto>>(badges);
    }
}
