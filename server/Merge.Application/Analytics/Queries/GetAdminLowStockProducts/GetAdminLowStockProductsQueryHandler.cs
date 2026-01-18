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

public class GetAdminLowStockProductsQueryHandler(
    IDbContext context,
    ILogger<GetAdminLowStockProductsQueryHandler> logger,
    IMapper mapper) : IRequestHandler<GetAdminLowStockProductsQuery, IEnumerable<ProductDto>>
{

    public async Task<IEnumerable<ProductDto>> Handle(GetAdminLowStockProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching admin low stock products. Threshold: {Threshold}", request.Threshold);

        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= request.Threshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<ProductDto>>(products);
    }
}

