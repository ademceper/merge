using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetStoreBySlug;

public class GetStoreBySlugQueryHandler(IDbContext context, IMapper mapper, ILogger<GetStoreBySlugQueryHandler> logger) : IRequestHandler<GetStoreBySlugQuery, StoreDto?>
{

    public async Task<StoreDto?> Handle(GetStoreBySlugQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting store by slug. Slug: {Slug}", request.Slug);

        var store = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Slug == request.Slug && s.Status == EntityStatus.Active, cancellationToken);

        if (store == null) return null;
        
        var dto = mapper.Map<StoreDto>(store);
        
        var productCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto with { ProductCount = productCount };
    }
}
