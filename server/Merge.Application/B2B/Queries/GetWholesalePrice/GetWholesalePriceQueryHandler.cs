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

public class GetWholesalePriceQueryHandler(
    IDbContext context,
    ILogger<GetWholesalePriceQueryHandler> logger) : IRequestHandler<GetWholesalePriceQuery, decimal?>
{

    public async Task<decimal?> Handle(GetWholesalePriceQuery request, CancellationToken cancellationToken)
    {
        // First try organization-specific pricing
        if (request.OrganizationId.HasValue)
        {
            var orgPrice = await context.Set<WholesalePrice>()
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
        var generalPrice = await context.Set<WholesalePrice>()
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

