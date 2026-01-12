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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetStoreQueryHandler : IRequestHandler<GetStoreQuery, StoreDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetStoreQueryHandler> _logger;

    public GetStoreQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetStoreQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<StoreDto?> Handle(GetStoreQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting store. StoreId: {StoreId}", request.StoreId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        // ✅ FIX: Record immutable - with expression kullan
        var productCount = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto with { ProductCount = productCount };
    }
}
