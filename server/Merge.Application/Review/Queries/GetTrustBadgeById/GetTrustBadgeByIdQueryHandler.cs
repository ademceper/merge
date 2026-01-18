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

namespace Merge.Application.Review.Queries.GetTrustBadgeById;

public class GetTrustBadgeByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetTrustBadgeByIdQueryHandler> logger) : IRequestHandler<GetTrustBadgeByIdQuery, TrustBadgeDto?>
{

    public async Task<TrustBadgeDto?> Handle(GetTrustBadgeByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching trust badge by Id: {BadgeId}", request.BadgeId);

        var badge = await context.Set<TrustBadge>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BadgeId, cancellationToken);

        if (badge == null)
        {
            logger.LogWarning("Trust badge not found with Id: {BadgeId}", request.BadgeId);
            return null;
        }

        return mapper.Map<TrustBadgeDto>(badge);
    }
}
