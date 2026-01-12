using AutoMapper;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Cart;

public class RecentlyViewedService : IRecentlyViewedService
{
    private readonly Merge.Application.Interfaces.IRepository<RecentlyViewedProduct> _recentlyViewedRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecentlyViewedService> _logger;
    private readonly CartSettings _cartSettings;

    public RecentlyViewedService(
        Merge.Application.Interfaces.IRepository<RecentlyViewedProduct> recentlyViewedRepository,
        IDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<RecentlyViewedService> logger,
        IOptions<CartSettings> cartSettings)
    {
        _recentlyViewedRepository = recentlyViewedRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<ProductDto>> GetRecentlyViewedAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted and !rvp.Product.IsDeleted checks (Global Query Filter handles it)
        var query = _context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
            .Include(rvp => rvp.Product)
                .ThenInclude(p => p.Category)
            .Where(rvp => rvp.UserId == userId && rvp.Product.IsActive);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var recentlyViewed = await query
            .OrderByDescending(rvp => rvp.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(rvp => rvp.Product)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<ProductDto>>(recentlyViewed);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task AddToRecentlyViewedAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var existing = await _context.Set<RecentlyViewedProduct>()
            .FirstOrDefaultAsync(rvp => rvp.UserId == userId &&
                                  rvp.ProductId == productId, cancellationToken);

        if (existing != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            existing.UpdateViewedAt();
            await _recentlyViewedRepository.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            // Yeni kayıt oluştur
            var recentlyViewed = RecentlyViewedProduct.Create(userId, productId);

            await _recentlyViewedRepository.AddAsync(recentlyViewed, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
            // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
            var count = await _context.Set<RecentlyViewedProduct>()
                .CountAsync(rvp => rvp.UserId == userId, cancellationToken);

            // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
            if (count > _cartSettings.MaxRecentlyViewedItems)
            {
                // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
                // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
                var oldest = await _context.Set<RecentlyViewedProduct>()
                    .Where(rvp => rvp.UserId == userId)
                    .OrderBy(rvp => rvp.ViewedAt)
                    .Take(count - _cartSettings.MaxRecentlyViewedItems)
                    .ToListAsync(cancellationToken);

                foreach (var item in oldest)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
            }
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task ClearRecentlyViewedAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Bulk delete instead of foreach DeleteAsync (N+1 fix)
        // ✅ PERFORMANCE: Removed manual !rvp.IsDeleted check (Global Query Filter handles it)
        var recentlyViewed = await _context.Set<RecentlyViewedProduct>()
            .Where(rvp => rvp.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var item in recentlyViewed)
        {
            item.IsDeleted = true;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken); // ✅ CRITICAL FIX: Single SaveChanges
    }
}

