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

    public async Task<StoreDto> CreateStoreAsync(Guid sellerId, CreateStoreDto dto)
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
            .FirstOrDefaultAsync(u => u.Id == sellerId);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // Generate slug
        var slug = GenerateSlug(dto.StoreName);
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var existingStore = await _context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug);

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
                .ToListAsync();

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

        await _context.Set<Store>().AddAsync(store);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
        store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == store.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var storeDto = _mapper.Map<StoreDto>(store!);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        storeDto.ProductCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id);
        
        return storeDto;
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid storeId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == storeId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        dto.ProductCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id);
        
        return dto;
    }

    public async Task<StoreDto?> GetStoreBySlugAsync(string slug)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Slug == slug && s.Status == EntityStatus.Active);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        dto.ProductCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id);
        
        return dto;
    }

    public async Task<IEnumerable<StoreDto>> GetSellerStoresAsync(Guid sellerId, string? status = null)
    {
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

        // ✅ PERFORMANCE: Batch load ProductCount (N+1 fix) - storeIds'i database'de oluştur
        var storeIds = await query.Select(s => s.Id).ToListAsync();
        
        var stores = await query
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.StoreName)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load ProductCount (N+1 fix)
        var productCounts = await _context.Products
            .AsNoTracking()
            .Where(p => p.StoreId.HasValue && storeIds.Contains(p.StoreId.Value))
            .GroupBy(p => p.StoreId)
            .Select(g => new { StoreId = g.Key!.Value, Count = g.Count() })
            .ToDictionaryAsync(x => x.StoreId, x => x.Count);

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
        
        return dtos;
    }

    public async Task<StoreDto?> GetPrimaryStoreAsync(Guid sellerId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && s.IsPrimary);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        if (store == null) return null;
        
        var dto = _mapper.Map<StoreDto>(store);
        
        // ✅ PERFORMANCE: ProductCount için database'de count (N+1 fix)
        dto.ProductCount = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id);
        
        return dto;
    }

    public async Task<bool> UpdateStoreAsync(Guid storeId, UpdateStoreDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId);

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
                .ToListAsync();

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
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteStoreAsync(Guid storeId)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId);

        if (store == null) return false;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // Check if store has products
        var hasProducts = await _context.Products
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == storeId);

        if (hasProducts)
        {
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        store.IsDeleted = true;
        store.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetPrimaryStoreAsync(Guid sellerId, Guid storeId)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.SellerId == sellerId);

        if (store == null) return false;

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        // Unset other primary stores
        var existingPrimary = await _context.Set<Store>()
            .Where(s => s.SellerId == sellerId && s.IsPrimary && s.Id != storeId)
            .ToListAsync();

        foreach (var s in existingPrimary)
        {
            s.IsPrimary = false;
        }

        store.IsPrimary = true;
        store.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VerifyStoreAsync(Guid storeId)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId);

        if (store == null) return false;

        store.IsVerified = true;
        store.VerifiedAt = DateTime.UtcNow;
        store.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendStoreAsync(Guid storeId, string reason)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId);

        if (store == null) return false;

        store.Status = EntityStatus.Suspended;
        store.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<StoreStatsDto> GetStoreStatsAsync(Guid storeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await _context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == storeId);

        if (store == null)
        {
            throw new NotFoundException("Mağaza", storeId);
        }

        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var activeProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId && p.IsActive);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .CountAsync();

        var totalRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SumAsync(oi => oi.TotalPrice);

        var monthlyRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .SumAsync(oi => oi.TotalPrice);

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var totalCustomers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync();

        // ✅ PERFORMANCE: Database'de average yap (memory'de işlem YASAK)
        var averageRating = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved &&
                  r.Product != null && r.Product.StoreId.HasValue && r.Product.StoreId.Value == storeId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;

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

