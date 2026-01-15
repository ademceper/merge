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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SharedWishlistDto> CreateSharedWishlistAsync(
        Guid userId, 
        CreateSharedWishlistDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var wishlist = SharedWishlist.Create(
            userId,
            dto.Name,
            dto.Description,
            dto.IsPublic);

        wishlist.GenerateShareCode();

        await _context.Set<SharedWishlist>().AddAsync(wishlist, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var productId in dto.ProductIds)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var item = SharedWishlistItem.Create(
                wishlist.Id,
                productId);
            await _context.Set<SharedWishlistItem>().AddAsync(item, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var created = await GetSharedWishlistByCodeAsync(wishlist.ShareCode, cancellationToken);
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<SharedWishlistDto?> GetSharedWishlistByCodeAsync(
        string shareCode,
        CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !w.IsDeleted (Global Query Filter)
        var wishlist = await _context.Set<SharedWishlist>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ShareCode == shareCode, cancellationToken);

        if (wishlist == null) return null;

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var items = await _context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.SharedWishlistId == wishlist.Id)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var baseDto = _mapper.Map<SharedWishlistDto>(wishlist);
        var mappedItems = _mapper.Map<List<SharedWishlistItemDto>>(items);
        return baseDto with { Items = mappedItems, ItemCount = items.Count };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<SharedWishlistDto>> GetMySharedWishlistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
        var wishlistsQuery = _context.Set<SharedWishlist>()
            .AsNoTracking()
            .Where(w => w.UserId == userId);

        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var wishlists = await wishlistsQuery
            .Include(w => w.User)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load items (N+1 fix) - subquery ile
        var wishlistIdsSubquery = from w in wishlistsQuery select w.Id;
        var itemsByWishlist = await _context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => wishlistIdsSubquery.Contains(i.SharedWishlistId))
            .GroupBy(i => i.SharedWishlistId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteSharedWishlistAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var wishlist = await _context.Set<SharedWishlist>()
            .FirstOrDefaultAsync(w => w.Id == wishlistId, cancellationToken);
        if (wishlist != null)
        {
            wishlist.MarkAsDeleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task MarkItemAsPurchasedAsync(
        Guid itemId, 
        Guid purchasedBy,
        CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: FindAsync yerine FirstOrDefaultAsync (Global Query Filter)
        var item = await _context.Set<SharedWishlistItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            item.MarkAsPurchased(purchasedBy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
