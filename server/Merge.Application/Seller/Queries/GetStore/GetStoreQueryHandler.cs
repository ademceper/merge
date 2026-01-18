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

namespace Merge.Application.Seller.Queries.GetStore;

public class GetStoreQueryHandler(IDbContext context, IMapper mapper, ILogger<GetStoreQueryHandler> logger) : IRequestHandler<GetStoreQuery, StoreDto?>
{

    public async Task<StoreDto?> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting store. StoreId: {StoreId}", request.StoreId);

        var store = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null) return null;
        
        var dto = mapper.Map<StoreDto>(store);
        
        var productCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto with { ProductCount = productCount };
    }
}
