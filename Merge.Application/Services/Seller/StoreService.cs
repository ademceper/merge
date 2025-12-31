using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
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

    public StoreService(ApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
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

        var seller = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == sellerId && !u.IsDeleted);

        if (seller == null)
        {
            throw new NotFoundException("Satıcı", sellerId);
        }

        // Generate slug
        var slug = GenerateSlug(dto.StoreName);
        var existingStore = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Slug == slug && !s.IsDeleted);

        if (existingStore != null)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks.ToString().Substring(10)}";
        }

        // If this is primary, unset other primary stores
        if (dto.IsPrimary)
        {
            var existingPrimary = await _context.Set<Store>()
                .Where(s => s.SellerId == sellerId && s.IsPrimary && !s.IsDeleted)
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
            Status = "Active",
            IsPrimary = dto.IsPrimary,
            Settings = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null
        };

        await _context.Set<Store>().AddAsync(store);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDto(store);
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid storeId)
    {
        var store = await _context.Set<Store>()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Id == storeId && !s.IsDeleted);

        return store != null ? await MapToDto(store) : null;
    }

    public async Task<StoreDto?> GetStoreBySlugAsync(string slug)
    {
        var store = await _context.Set<Store>()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.Slug == slug && !s.IsDeleted && s.Status == "Active");

        return store != null ? await MapToDto(store) : null;
    }

    public async Task<IEnumerable<StoreDto>> GetSellerStoresAsync(Guid sellerId, string? status = null)
    {
        var query = _context.Set<Store>()
            .Include(s => s.Seller)
            .Where(s => s.SellerId == sellerId && !s.IsDeleted);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status == status);
        }

        var stores = await query
            .OrderByDescending(s => s.IsPrimary)
            .ThenBy(s => s.StoreName)
            .ToListAsync();

        var result = new List<StoreDto>();
        foreach (var store in stores)
        {
            result.Add(await MapToDto(store));
        }
        return result;
    }

    public async Task<StoreDto?> GetPrimaryStoreAsync(Guid sellerId)
    {
        var store = await _context.Set<Store>()
            .Include(s => s.Seller)
            .FirstOrDefaultAsync(s => s.SellerId == sellerId && s.IsPrimary && !s.IsDeleted);

        return store != null ? await MapToDto(store) : null;
    }

    public async Task<bool> UpdateStoreAsync(Guid storeId, UpdateStoreDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && !s.IsDeleted);

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
            store.Status = dto.Status;
        }

        if (dto.IsPrimary.HasValue && dto.IsPrimary.Value)
        {
            // Unset other primary stores
            var existingPrimary = await _context.Set<Store>()
                .Where(s => s.SellerId == store.SellerId && s.IsPrimary && s.Id != storeId && !s.IsDeleted)
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
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && !s.IsDeleted);

        if (store == null) return false;

        // Check if store has products
        var hasProducts = await _context.Products
            .AnyAsync(p => p.StoreId == storeId && !p.IsDeleted);

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
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && s.SellerId == sellerId && !s.IsDeleted);

        if (store == null) return false;

        // Unset other primary stores
        var existingPrimary = await _context.Set<Store>()
            .Where(s => s.SellerId == sellerId && s.IsPrimary && s.Id != storeId && !s.IsDeleted)
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
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && !s.IsDeleted);

        if (store == null) return false;

        store.IsVerified = true;
        store.VerifiedAt = DateTime.UtcNow;
        store.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendStoreAsync(Guid storeId, string reason)
    {
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && !s.IsDeleted);

        if (store == null) return false;

        store.Status = "Suspended";
        store.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<StoreStatsDto> GetStoreStatsAsync(Guid storeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var store = await _context.Set<Store>()
            .FirstOrDefaultAsync(s => s.Id == storeId && !s.IsDeleted);

        if (store == null)
        {
            throw new NotFoundException("Mağaza", storeId);
        }

        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var totalProducts = await _context.Products
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId && !p.IsDeleted);

        var activeProducts = await _context.Products
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == storeId && !p.IsDeleted && p.IsActive);

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => !o.IsDeleted &&
                  o.PaymentStatus == "Paid" &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == storeId))
            .ToListAsync();

        var totalOrders = orders.Count;
        var totalRevenue = orders.Sum(o => o.OrderItems
            .Where(oi => oi.Product != null && oi.Product.StoreId.HasValue && oi.Product.StoreId.Value == storeId)
            .Sum(oi => oi.TotalPrice));

        var monthlyOrders = orders
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToList();

        var monthlyRevenue = monthlyOrders.Sum(o => o.OrderItems
            .Where(oi => oi.Product != null && oi.Product.StoreId.HasValue && oi.Product.StoreId.Value == storeId)
            .Sum(oi => oi.TotalPrice));

        var totalCustomers = orders
            .Select(o => o.UserId)
            .Distinct()
            .Count();

        var reviews = await _context.Reviews
            .Include(r => r.Product)
            .Where(r => !r.IsDeleted &&
                  r.IsApproved &&
                  r.Product != null && r.Product.StoreId.HasValue && r.Product.StoreId.Value == storeId)
            .ToListAsync();

        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

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

    private async Task<StoreDto> MapToDto(Store store)
    {
        await _context.Entry(store)
            .Reference(s => s.Seller)
            .LoadAsync();

        var productCount = await _context.Products
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == store.Id && !p.IsDeleted);

        return new StoreDto
        {
            Id = store.Id,
            SellerId = store.SellerId,
            SellerName = store.Seller != null 
                ? $"{store.Seller.FirstName} {store.Seller.LastName}" 
                : string.Empty,
            StoreName = store.StoreName,
            Slug = store.Slug,
            Description = store.Description,
            LogoUrl = store.LogoUrl,
            BannerUrl = store.BannerUrl,
            ContactEmail = store.ContactEmail,
            ContactPhone = store.ContactPhone,
            Address = store.Address,
            City = store.City,
            Country = store.Country,
            Status = store.Status,
            IsPrimary = store.IsPrimary,
            IsVerified = store.IsVerified,
            VerifiedAt = store.VerifiedAt,
            Settings = !string.IsNullOrEmpty(store.Settings)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(store.Settings)
                : null,
            ProductCount = productCount,
            CreatedAt = store.CreatedAt
        };
    }
}

