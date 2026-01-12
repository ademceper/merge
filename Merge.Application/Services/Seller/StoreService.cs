using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserEntity = Merge.Domain.Modules.Identity.User;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
namespace Merge.Application.Services.Seller;

public class StoreService : IStoreService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StoreService> _logger;
    private readonly ServiceSettings _serviceSettings;
    private readonly PaginationSettings _paginationSettings;

    public StoreService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<StoreService> logger,
        IOptions<ServiceSettings> serviceSettings,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _serviceSettings = serviceSettings.Value;
        _paginationSettings = paginationSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StoreDto> CreateStoreAsync(Guid sellerId, CreateStoreDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.StoreName))
        {
            throw new ValidationException("Mağaza adı boş olamaz.");
        }

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var seller = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == sellerId, cancellationToken);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // Generate slug
        var slug = GenerateSlug(dto.StoreName);
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var existingStore = await _context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

        if (existingStore != null)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";
        }

        // If this is primary, unset other primary stores
        if (dto.IsPrimary)
        {
            // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
            var existingPrimary = await _context.Set<Store>()
                .Where(s => s.SellerId == sellerId && s.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var primaryStore in existingPrimary)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                primaryStore.RemovePrimaryStatus();
            }
        }

        var settingsJson = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var store = Store.Create(
            sellerId: sellerId,
            storeName: dto.StoreName,
            description: dto.Description,
            logoUrl: dto.LogoUrl,
            bannerUrl: dto.BannerUrl,
            contactEmail: dto.ContactEmail,
            contactPhone: dto.ContactPhone,
            address: dto.Address,
            city: dto.City,
            country: dto.Country,
            postalCode: dto.PostalCode,
            settings: settingsJson
        );

        // Set as primary if requested
        if (dto.IsPrimary)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            store.SetAsPrimary();
        }

        await _context.Set<Store>().AddAsync(store, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        var reloadedStore = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);

        if (reloadedStore == null)
        {
            _logger.LogWarning("Store {StoreId} not found after creation", store.Id);
            return _mapper.Map<StoreDto>(store);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var storeDto = _mapper.Map<StoreDto>(reloadedStore);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        storeDto.ProductCount = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == reloadedStore.Id, cancellationToken);
        
        return storeDto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StoreDto?> GetStoreByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        dto.ProductCount = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StoreDto?> GetStoreBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Slug == slug && s.Status == EntityStatus.Active, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        dto.ProductCount = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public async Task<PagedResult<StoreDto>> GetSellerStoresAsync(Guid sellerId, EntityStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number config'den - PaginationSettings kullanımı
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<Store> query = _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Where(s => s.SellerId == sellerId);

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
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
        
        // ✅ FIX: ProductCount set et
        foreach (var dto in dtos)
        {
            if (productCounts.TryGetValue(dto.Id, out var count))
            {
                dto.ProductCount = count;
            }
        }
        
        return new PagedResult<StoreDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StoreDto?> GetPrimaryStoreAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && s.IsPrimary, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        dto.ProductCount = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateStoreAsync(Guid storeId, UpdateStoreDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.UpdateDetails(
            storeName: !string.IsNullOrEmpty(dto.StoreName) ? dto.StoreName : null,
            description: dto.Description,
            logoUrl: dto.LogoUrl,
            bannerUrl: dto.BannerUrl,
            contactEmail: dto.ContactEmail,
            contactPhone: dto.ContactPhone,
            address: dto.Address,
            city: dto.City,
            country: dto.Country,
            postalCode: dto.PostalCode,
            settings: dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null
        );

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        if (dto.Status.HasValue)
        {
            var newStatus = dto.Status.Value;
            if (newStatus == EntityStatus.Active && store.Status != EntityStatus.Active)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                store.Activate();
            }
            else if (newStatus == EntityStatus.Suspended && store.Status != EntityStatus.Suspended)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                store.Suspend();
            }
        }

        if (dto.IsPrimary.HasValue && dto.IsPrimary.Value)
        {
            // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
            // Unset other primary stores
            var existingPrimary = await _context.Set<Store>()
                .Where(s => s.SellerId == store.SellerId && s.IsPrimary && s.Id != storeId)
                .ToListAsync(cancellationToken);

            foreach (var s in existingPrimary)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                s.RemovePrimaryStatus();
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            store.SetAsPrimary();
        }
        else if (dto.IsPrimary.HasValue && !dto.IsPrimary.Value)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            store.RemovePrimaryStatus();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteStoreAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Check if store has products
        var hasProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == storeId, cancellationToken);

        if (hasProducts)
        {
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Delete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SetPrimaryStoreAsync(Guid sellerId, Guid storeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.SellerId == sellerId, cancellationToken);

        if (store == null) return false;

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        // Unset other primary stores
        var existingPrimary = await _context.Set<Store>()
            .Where(s => s.SellerId == sellerId && s.IsPrimary && s.Id != storeId)
            .ToListAsync(cancellationToken);

        foreach (var s in existingPrimary)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            s.RemovePrimaryStatus();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.SetAsPrimary();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> VerifyStoreAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Verify();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SuspendStoreAsync(Guid storeId, string reason, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        store.Suspend();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<StoreStatsDto> GetStoreStatsAsync(Guid storeId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null)
        {
            throw new NotFoundException("Mağaza", storeId);
        }

        startDate ??= DateTime.UtcNow.AddDays(-_serviceSettings.DefaultDateRangeDays); // ✅ BOLUM 12.0: Magic number config'den
        endDate ??= DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var activeProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId && p.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .CountAsync(cancellationToken);

        var totalRevenue = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var monthlyRevenue = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var totalCustomers = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de average yap (memory'de işlem YASAK)
        var averageRating = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved &&
                  r.Product != null && r.Product.StoreId.HasValue && r.Product.StoreId.Value == storeId)
            .AverageAsync(r => (double?)r.Rating, cancellationToken) ?? 0;

        return new StoreStatsDto
        {
            StoreId = storeId,
            StoreName = store.StoreName,
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            TotalOrders = totalOrders,
            TotalRevenue = Math.Round(totalRevenue, 2),
            MonthlyRevenue = Math.Round(monthlyRevenue, 2),
            TotalCustomers = totalCustomers,
            AverageRating = Math.Round((decimal)averageRating, 2)
        };
    }

    private string GenerateSlug(string text)
    {
        var slug = text.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}

