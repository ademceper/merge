using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Marketing;

public class SharedWishlistService : ISharedWishlistService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SharedWishlistService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SharedWishlistDto> CreateSharedWishlistAsync(Guid userId, CreateSharedWishlistDto dto)
    {
        var wishlist = new SharedWishlist
        {
            UserId = userId,
            ShareCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            Name = dto.Name,
            Description = dto.Description,
            IsPublic = dto.IsPublic
        };

        await _context.Set<SharedWishlist>().AddAsync(wishlist);
        await _unitOfWork.SaveChangesAsync();

        foreach (var productId in dto.ProductIds)
        {
            var item = new SharedWishlistItem
            {
                SharedWishlistId = wishlist.Id,
                ProductId = productId
            };
            await _context.Set<SharedWishlistItem>().AddAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();
        var created = await GetSharedWishlistByCodeAsync(wishlist.ShareCode);
        if (created == null)
        {
            var items = _mapper.Map<List<SharedWishlistItemDto>>(dto.ProductIds.Select(id => new { ProductId = id }));
            return new SharedWishlistDto(
                wishlist.Id,
                wishlist.ShareCode,
                wishlist.Name,
                wishlist.Description,
                wishlist.IsPublic,
                0,
                dto.ProductIds.Count,
                items);
        }
        return created;
    }

    public async Task<SharedWishlistDto?> GetSharedWishlistByCodeAsync(string shareCode)
    {
        // ✅ PERFORMANCE: Removed manual !w.IsDeleted (Global Query Filter)
        var wishlist = await _context.Set<SharedWishlist>()
            .Include(w => w.User)
            .FirstOrDefaultAsync(w => w.ShareCode == shareCode);

        if (wishlist == null)
            return null;

        wishlist.ViewCount++;
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !i.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var items = await _context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.SharedWishlistId == wishlist.Id)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var baseDto = _mapper.Map<SharedWishlistDto>(wishlist);
        var mappedItems = _mapper.Map<List<SharedWishlistItemDto>>(items);
        return baseDto with { Items = mappedItems, ItemCount = items.Count };
    }

    public async Task<IEnumerable<SharedWishlistDto>> GetMySharedWishlistsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !w.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var wishlists = await _context.Set<SharedWishlist>()
            .AsNoTracking()
            .Include(w => w.User)
            .Where(w => w.UserId == userId)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch load items (N+1 fix)
        // ✅ PERFORMANCE: wishlistIds'i memory'den al (zaten yüklenmiş wishlists'ten)
        var wishlistIds = wishlists.Select(w => w.Id).ToList();
        
        // ✅ PERFORMANCE: Database'de GroupBy ve ToDictionaryAsync yap (ToListAsync() sonrası memory'de işlem YASAK)
        var itemsByWishlist = await _context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => wishlistIds.Contains(i.SharedWishlistId))
            .GroupBy(i => i.SharedWishlistId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = new List<SharedWishlistDto>();
        foreach (var wishlist in wishlists)
        {
            var baseDto = _mapper.Map<SharedWishlistDto>(wishlist);
            if (itemsByWishlist.TryGetValue(wishlist.Id, out var wishlistItems))
            {
                var mappedItems = _mapper.Map<List<SharedWishlistItemDto>>(wishlistItems);
                result.Add(baseDto with { Items = mappedItems, ItemCount = wishlistItems.Count });
            }
            else
            {
                result.Add(baseDto with { Items = new List<SharedWishlistItemDto>(), ItemCount = 0 });
            }
        }

        return result;
    }

    public async Task DeleteSharedWishlistAsync(Guid wishlistId)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var wishlist = await _context.Set<SharedWishlist>()
            .FirstOrDefaultAsync(w => w.Id == wishlistId);
        if (wishlist != null)
        {
            wishlist.MarkAsDeleted();
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MarkItemAsPurchasedAsync(Guid itemId, Guid purchasedBy)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var item = await _context.Set<SharedWishlistItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId);
        if (item != null)
        {
            item.IsPurchased = true;
            item.PurchasedBy = purchasedBy;
            item.PurchasedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
