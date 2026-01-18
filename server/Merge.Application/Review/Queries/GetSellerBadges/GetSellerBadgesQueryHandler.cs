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

public class GetSellerBadgesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSellerBadgesQueryHandler> logger) : IRequestHandler<GetSellerBadgesQuery, IEnumerable<SellerTrustBadgeDto>>
{

    public async Task<IEnumerable<SellerTrustBadgeDto>> Handle(GetSellerBadgesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching seller badges. SellerId: {SellerId}", request.SellerId);

        var badges = await context.Set<SellerTrustBadge>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(stb => stb.TrustBadge)
            .Include(stb => stb.Seller)
            .Where(stb => stb.SellerId == request.SellerId && stb.IsActive)
            .OrderBy(stb => stb.TrustBadge.DisplayOrder)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<SellerTrustBadgeDto>>(badges);
    }
}
