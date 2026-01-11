using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Review.Queries.GetTrustBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTrustBadgesQueryHandler : IRequestHandler<GetTrustBadgesQuery, IEnumerable<TrustBadgeDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTrustBadgesQueryHandler> _logger;

    public GetTrustBadgesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetTrustBadgesQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<TrustBadgeDto>> Handle(GetTrustBadgesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching trust badges. BadgeType: {BadgeType}", request.BadgeType);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var query = _context.Set<TrustBadge>()
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
        return _mapper.Map<IEnumerable<TrustBadgeDto>>(badges);
    }
}
