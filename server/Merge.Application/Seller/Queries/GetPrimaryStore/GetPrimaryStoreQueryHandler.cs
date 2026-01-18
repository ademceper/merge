using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetPrimaryStore;

public class GetPrimaryStoreQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPrimaryStoreQueryHandler> logger) : IRequestHandler<GetPrimaryStoreQuery, StoreDto?>
{

    public async Task<StoreDto?> Handle(GetPrimaryStoreQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting primary store. SellerId: {SellerId}", request.SellerId);

        var store = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.SellerId == request.SellerId && s.IsPrimary, cancellationToken);

        if (store is null) return null;
        
        var dto = mapper.Map<StoreDto>(store);
        
        var productCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto with { ProductCount = productCount };
    }
}
