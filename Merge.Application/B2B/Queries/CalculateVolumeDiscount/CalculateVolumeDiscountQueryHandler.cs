using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.B2B.Queries.CalculateVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CalculateVolumeDiscountQueryHandler : IRequestHandler<CalculateVolumeDiscountQuery, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<CalculateVolumeDiscountQueryHandler> _logger;

    public CalculateVolumeDiscountQueryHandler(
        IDbContext context,
        ILogger<CalculateVolumeDiscountQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(CalculateVolumeDiscountQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !vd.IsDeleted check (Global Query Filter handles it)
        // First try organization-specific discount
        if (request.OrganizationId.HasValue)
        {
            var orgDiscount = await _context.Set<VolumeDiscount>()
                .AsNoTracking()
                .Where(vd => (vd.ProductId == request.ProductId || vd.CategoryId != null) &&
                           vd.OrganizationId == request.OrganizationId.Value &&
                           vd.MinQuantity <= request.Quantity &&
                           (vd.MaxQuantity == null || vd.MaxQuantity >= request.Quantity) &&
                           vd.IsActive &&
                           (vd.StartDate == null || vd.StartDate <= DateTime.UtcNow) &&
                           (vd.EndDate == null || vd.EndDate >= DateTime.UtcNow))
                .OrderByDescending(vd => vd.MinQuantity)
                .FirstOrDefaultAsync(cancellationToken);

            if (orgDiscount != null)
            {
                return orgDiscount.DiscountPercentage;
            }
        }

        // Fall back to general discount
        var generalDiscount = await _context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Where(vd => (vd.ProductId == request.ProductId || vd.CategoryId != null) &&
                       vd.OrganizationId == null &&
                       vd.MinQuantity <= request.Quantity &&
                       (vd.MaxQuantity == null || vd.MaxQuantity >= request.Quantity) &&
                       vd.IsActive &&
                       (vd.StartDate == null || vd.StartDate <= DateTime.UtcNow) &&
                       (vd.EndDate == null || vd.EndDate >= DateTime.UtcNow))
            .OrderByDescending(vd => vd.MinQuantity)
            .FirstOrDefaultAsync(cancellationToken);

        return generalDiscount?.DiscountPercentage ?? 0;
    }
}

