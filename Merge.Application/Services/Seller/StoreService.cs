using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using System.Text.RegularExpressions;
using Merge.Application.DTOs.Seller;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
namespace Merge.Application.Services.Seller;

public class StoreService : IStoreService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StoreService> _logger;

    public StoreService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<StoreService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
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
                primaryStore.IsPrimary = false;
            }
        }

        var store = new Store
        {
            SellerId = sellerId,
            StoreName = dto.StoreName,
            Slug = slug,
            Description = dto.Description,
            LogoUrl = dto.LogoUrl,
            BannerUrl = dto.BannerUrl,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            Status = EntityStatus.Active,
            IsPrimary = dto.IsPrimary,
            Settings = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null
        };

        await _context.Set<Store>().AddAsync(store, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var storeDto = _mapper.Map<StoreDto>(store!);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        storeDto.ProductCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
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
        dto.ProductCount = await _context.Products
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
        dto.ProductCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<StoreDto>> GetSellerStoresAsync(Guid sellerId, string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        IQueryable<Store> query = _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Where(s => s.SellerId == sellerId);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<EntityStatus>(status);
            query = query.Where(s => s.Status == statusEnum);
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
        var productCounts = await _context.Products
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
        dto.ProductCount = await _context.Products
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

        if (!string.IsNullOrEmpty(dto.StoreName))
        {
            store.StoreName = dto.StoreName;
        }

        if (dto.Description != null)
        {
            store.Description = dto.Description;
        }

        if (dto.LogoUrl != null)
        {
            store.LogoUrl = dto.LogoUrl;
        }

        if (dto.BannerUrl != null)
        {
            store.BannerUrl = dto.BannerUrl;
        }

        if (dto.ContactEmail != null)
        {
            store.ContactEmail = dto.ContactEmail;
        }

        if (dto.ContactPhone != null)
        {
            store.ContactPhone = dto.ContactPhone;
        }

        if (dto.Address != null)
        {
            store.Address = dto.Address;
        }

        if (dto.City != null)
        {
            store.City = dto.City;
        }

        if (dto.Country != null)
        {
            store.Country = dto.Country;
        }

        if (!string.IsNullOrEmpty(dto.Status))
        {
            store.Status = Enum.Parse<EntityStatus>(dto.Status);
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
                s.IsPrimary = false;
            }

            store.IsPrimary = true;
        }
        else if (dto.IsPrimary.HasValue && !dto.IsPrimary.Value)
        {
            store.IsPrimary = false;
        }

        if (dto.Settings != null)
        {
            store.Settings = JsonSerializer.Serialize(dto.Settings);
        }

        store.UpdatedAt = DateTime.UtcNow;
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
        var hasProducts = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == storeId, cancellationToken);

        if (hasProducts)
        {
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        store.IsDeleted = true;
        store.UpdatedAt = DateTime.UtcNow;
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
            s.IsPrimary = false;
        }

        store.IsPrimary = true;
        store.UpdatedAt = DateTime.UtcNow;
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

        store.IsVerified = true;
        store.VerifiedAt = DateTime.UtcNow;
        store.UpdatedAt = DateTime.UtcNow;
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

        store.Status = EntityStatus.Suspended;
        store.UpdatedAt = DateTime.UtcNow;
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

        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var activeProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId && p.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .CountAsync(cancellationToken);

        var totalRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var monthlyRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var totalCustomers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de average yap (memory'de işlem YASAK)
        var averageRating = await _context.Reviews
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

