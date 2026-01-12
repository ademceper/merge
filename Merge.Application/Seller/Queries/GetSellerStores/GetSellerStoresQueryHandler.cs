using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerStores;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerStoresQueryHandler : IRequestHandler<GetSellerStoresQuery, PagedResult<StoreDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSellerStoresQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetSellerStoresQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSellerStoresQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<StoreDto>> Handle(GetSellerStoresQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting seller stores. SellerId: {SellerId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.SellerId, request.Status?.ToString() ?? "All", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize 
            ? _paginationSettings.MaxPageSize 
            : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<Store> query = _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Where(s => s.SellerId == request.SellerId);

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load ProductCount (N+1 fix) - storeIds'i database'de oluştur
        var storeIds = await query
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.StoreName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);
        
        var stores = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Where(s => storeIds.Contains(s.Id))
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.StoreName)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load ProductCount (N+1 fix)
        var productCounts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.StoreId.HasValue && storeIds.Contains(p.StoreId.Value))
            .GroupBy(p => p.StoreId)
            .Select(g => new { StoreId = g.Key!.Value, Count = g.Count() })
            .ToDictionaryAsync(x => x.StoreId, x => x.Count, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dtos = _mapper.Map<IEnumerable<StoreDto>>(stores).ToList();
        
        // ✅ FIX: Record immutable - with expression kullan
        var dtosWithProductCount = dtos.Select(dto =>
        {
            if (productCounts.TryGetValue(dto.Id, out var count))
            {
                return dto with { ProductCount = count };
            }
            return dto;
        }).ToList();
        
        return new PagedResult<StoreDto>
        {
            Items = dtosWithProductCount,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
