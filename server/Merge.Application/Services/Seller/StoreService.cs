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

namespace Merge.Application.Services.Seller;

public class StoreService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<StoreService> logger, IOptions<ServiceSettings> serviceSettings, IOptions<PaginationSettings> paginationSettings) : IStoreService
{
    private readonly ServiceSettings serviceConfig = serviceSettings.Value;
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<StoreDto> CreateStoreAsync(Guid sellerId, CreateStoreDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.StoreName))
        {
            throw new ValidationException("Mağaza adı boş olamaz.");
        }

        var seller = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == sellerId, cancellationToken);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // Generate slug
        var slug = GenerateSlug(dto.StoreName);
        var existingStore = await context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

        if (existingStore != null)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";
        }

        // If this is primary, unset other primary stores
        if (dto.IsPrimary)
        {
            var existingPrimary = await context.Set<Store>()
                .Where(s => s.SellerId == sellerId && s.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var primaryStore in existingPrimary)
            {
                primaryStore.RemovePrimaryStatus();
            }
        }

        var settingsJson = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null;

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
            store.SetAsPrimary();
        }

        await context.Set<Store>().AddAsync(store, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var reloadedStore = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == store.Id, cancellationToken);

        if (reloadedStore == null)
        {
            logger.LogWarning("Store {StoreId} not found after creation", store.Id);
            return mapper.Map<StoreDto>(store);
        }

        var storeDto = mapper.Map<StoreDto>(reloadedStore);
        
        storeDto.ProductCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == reloadedStore.Id, cancellationToken);
        
        return storeDto;
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return null;
        
        var dto = mapper.Map<StoreDto>(store);
        
        dto.ProductCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    public async Task<StoreDto?> GetStoreBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Slug == slug && s.Status == EntityStatus.Active, cancellationToken);

        if (store == null) return null;
        
        var dto = mapper.Map<StoreDto>(store);
        
        dto.ProductCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    public async Task<PagedResult<StoreDto>> GetSellerStoresAsync(Guid sellerId, EntityStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<Store> query = context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Where(s => s.SellerId == sellerId);

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var storeIds = await query
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.StoreName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);
        
        var stores = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Where(s => storeIds.Contains(s.Id))
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.StoreName)
            .ToListAsync(cancellationToken);

        var productCounts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.StoreId.HasValue && storeIds.Contains(p.StoreId.Value))
            .GroupBy(p => p.StoreId)
            .Select(g => new { StoreId = g.Key!.Value, Count = g.Count() })
            .ToDictionaryAsync(x => x.StoreId, x => x.Count, cancellationToken);

        var dtos = mapper.Map<IEnumerable<StoreDto>>(stores).ToList();
        
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

    public async Task<StoreDto?> GetPrimaryStoreAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && s.IsPrimary, cancellationToken);

        if (store == null) return null;
        
        var dto = mapper.Map<StoreDto>(store);
        
        dto.ProductCount = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id, cancellationToken);
        
        return dto;
    }

    public async Task<bool> UpdateStoreAsync(Guid storeId, UpdateStoreDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

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

        if (dto.Status.HasValue)
        {
            var newStatus = dto.Status.Value;
            if (newStatus == EntityStatus.Active && store.Status != EntityStatus.Active)
            {
                store.Activate();
            }
            else if (newStatus == EntityStatus.Suspended && store.Status != EntityStatus.Suspended)
            {
                store.Suspend();
            }
        }

        if (dto.IsPrimary.HasValue && dto.IsPrimary.Value)
        {
            // Unset other primary stores
            var existingPrimary = await context.Set<Store>()
                .Where(s => s.SellerId == store.SellerId && s.IsPrimary && s.Id != storeId)
                .ToListAsync(cancellationToken);

            foreach (var s in existingPrimary)
            {
                s.RemovePrimaryStatus();
            }

            store.SetAsPrimary();
        }
        else if (dto.IsPrimary.HasValue && !dto.IsPrimary.Value)
        {
            store.RemovePrimaryStatus();
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteStoreAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        // Check if store has products
        var hasProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.StoreId == storeId, cancellationToken);

        if (hasProducts)
        {
            throw new BusinessException("Ürünleri olan bir mağaza silinemez. Önce ürünleri kaldırın veya transfer edin.");
        }

        store.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SetPrimaryStoreAsync(Guid sellerId, Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.SellerId == sellerId, cancellationToken);

        if (store == null) return false;

        // Unset other primary stores
        var existingPrimary = await context.Set<Store>()
            .Where(s => s.SellerId == sellerId && s.IsPrimary && s.Id != storeId)
            .ToListAsync(cancellationToken);

        foreach (var s in existingPrimary)
        {
            s.RemovePrimaryStatus();
        }

        store.SetAsPrimary();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> VerifyStoreAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        store.Verify();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SuspendStoreAsync(Guid storeId, string reason, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        store.Suspend();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<StoreStatsDto> GetStoreStatsAsync(Guid storeId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var store = await context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null)
        {
            throw new NotFoundException("Mağaza", storeId);
        }

        startDate ??= DateTime.UtcNow.AddDays(-serviceConfig.DefaultDateRangeDays); // ✅ BOLUM 12.0: Magic number config'den
        endDate ??= DateTime.UtcNow;

        var totalProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId, cancellationToken);

        var activeProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId && p.IsActive, cancellationToken);

        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .CountAsync(cancellationToken);

        var totalRevenue = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  p.StoreId.HasValue && p.StoreId.Value == storeId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var monthlyRevenue = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.StoreId.HasValue && p.StoreId.Value == storeId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var totalCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var averageRating = await context.Set<ReviewEntity>()
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

