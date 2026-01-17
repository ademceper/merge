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

namespace Merge.Application.Review.Queries.GetProductBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetProductBadgesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetProductBadgesQueryHandler> logger) : IRequestHandler<GetProductBadgesQuery, IEnumerable<ProductTrustBadgeDto>>
{

    public async Task<IEnumerable<ProductTrustBadgeDto>> Handle(GetProductBadgesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product badges. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !ptb.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var badges = await context.Set<ProductTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ptb => ptb.TrustBadge)
            .Include(ptb => ptb.Product)
            .Where(ptb => ptb.ProductId == request.ProductId && ptb.IsActive)
            .OrderBy(ptb => ptb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<IEnumerable<ProductTrustBadgeDto>>(badges);
    }
}
