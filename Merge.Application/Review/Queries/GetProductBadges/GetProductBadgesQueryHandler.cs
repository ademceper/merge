using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Review.Queries.GetProductBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetProductBadgesQueryHandler : IRequestHandler<GetProductBadgesQuery, IEnumerable<ProductTrustBadgeDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProductBadgesQueryHandler> _logger;

    public GetProductBadgesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetProductBadgesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductTrustBadgeDto>> Handle(GetProductBadgesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product badges. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ptb.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var badges = await _context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .Where(ptb => ptb.ProductId == request.ProductId && ptb.IsActive)
            .OrderBy(ptb => ptb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ProductTrustBadgeDto>>(badges);
    }
}
