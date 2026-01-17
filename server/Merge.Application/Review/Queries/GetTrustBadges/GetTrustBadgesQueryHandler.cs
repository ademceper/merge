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

namespace Merge.Application.Review.Queries.GetTrustBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTrustBadgesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetTrustBadgesQueryHandler> logger) : IRequestHandler<GetTrustBadgesQuery, IEnumerable<TrustBadgeDto>>
{

    public async Task<IEnumerable<TrustBadgeDto>> Handle(GetTrustBadgesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching trust badges. BadgeType: {BadgeType}", request.BadgeType);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var query = context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive);

        if (!string.IsNullOrEmpty(request.BadgeType))
        {
            query = query.Where(b => b.BadgeType == request.BadgeType);
        }

        var badges = await query
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<IEnumerable<TrustBadgeDto>>(badges);
    }
}
