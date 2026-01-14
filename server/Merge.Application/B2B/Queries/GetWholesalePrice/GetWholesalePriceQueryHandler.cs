using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetWholesalePriceQueryHandler : IRequestHandler<GetWholesalePriceQuery, decimal?>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetWholesalePriceQueryHandler> _logger;

    public GetWholesalePriceQueryHandler(
        IDbContext context,
        ILogger<GetWholesalePriceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal?> Handle(GetWholesalePriceQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !wp.IsDeleted check (Global Query Filter handles it)
        // First try organization-specific pricing
        if (request.OrganizationId.HasValue)
        {
            var orgPrice = await _context.Set<WholesalePrice>()
                .AsNoTracking()
                .Where(wp => wp.ProductId == request.ProductId &&
                           wp.OrganizationId == request.OrganizationId.Value &&
                           wp.MinQuantity <= request.Quantity &&
                           (wp.MaxQuantity == null || wp.MaxQuantity >= request.Quantity) &&
                           wp.IsActive &&
                           (wp.StartDate == null || wp.StartDate <= DateTime.UtcNow) &&
                           (wp.EndDate == null || wp.EndDate >= DateTime.UtcNow))
                .OrderByDescending(wp => wp.MinQuantity)
                .FirstOrDefaultAsync(cancellationToken);

            if (orgPrice != null)
            {
                return orgPrice.Price;
            }
        }

        // Fall back to general pricing
        var generalPrice = await _context.Set<WholesalePrice>()
            .AsNoTracking()
            .Where(wp => wp.ProductId == request.ProductId &&
                       wp.OrganizationId == null &&
                       wp.MinQuantity <= request.Quantity &&
                       (wp.MaxQuantity == null || wp.MaxQuantity >= request.Quantity) &&
                       wp.IsActive &&
                       (wp.StartDate == null || wp.StartDate <= DateTime.UtcNow) &&
                       (wp.EndDate == null || wp.EndDate >= DateTime.UtcNow))
            .OrderByDescending(wp => wp.MinQuantity)
            .FirstOrDefaultAsync(cancellationToken);

        return generalPrice?.Price;
    }
}

