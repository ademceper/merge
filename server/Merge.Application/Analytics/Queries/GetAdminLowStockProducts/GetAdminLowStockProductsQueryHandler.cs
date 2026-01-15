using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAdminLowStockProductsQueryHandler(
    IDbContext context,
    ILogger<GetAdminLowStockProductsQueryHandler> logger,
    IMapper mapper) : IRequestHandler<GetAdminLowStockProductsQuery, IEnumerable<ProductDto>>
{

    public async Task<IEnumerable<ProductDto>> Handle(GetAdminLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching admin low stock products. Threshold: {Threshold}", request.Threshold);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= request.Threshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return mapper.Map<IEnumerable<ProductDto>>(products);
    }
}

