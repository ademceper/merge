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

public class SharedWishlistService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper) : ISharedWishlistService
{

    public async Task<SharedWishlistDto> CreateSharedWishlistAsync(
        Guid userId, 
        CreateSharedWishlistDto dto,
        CancellationToken cancellationToken = default)
    {
        var wishlist = SharedWishlist.Create(
            userId,
            dto.Name,
            dto.Description,
            dto.IsPublic);

        wishlist.GenerateShareCode();

        await context.Set<SharedWishlist>().AddAsync(wishlist, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var productId in dto.ProductIds)
        {
            var item = SharedWishlistItem.Create(
                wishlist.Id,
                productId);
            await context.Set<SharedWishlistItem>().AddAsync(item, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var created = await GetSharedWishlistByCodeAsync(wishlist.ShareCode, cancellationToken);
        if (created is null)
        {
            var items = mapper.Map<List<SharedWishlistItemDto>>(dto.ProductIds.Select(id => new { ProductId = id }));
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

    public async Task<SharedWishlistDto?> GetSharedWishlistByCodeAsync(
        string shareCode,
        CancellationToken cancellationToken = default)
    {
        var wishlist = await context.Set<SharedWishlist>()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.ShareCode == shareCode, cancellationToken);

        if (wishlist is null) return null;

        var items = await context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => i.SharedWishlistId == wishlist.Id)
            .ToListAsync(cancellationToken);

        var baseDto = mapper.Map<SharedWishlistDto>(wishlist);
        var mappedItems = mapper.Map<List<SharedWishlistItemDto>>(items);
        return baseDto with { Items = mappedItems, ItemCount = items.Count };
    }

    public async Task<IEnumerable<SharedWishlistDto>> GetMySharedWishlistsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var wishlistsQuery = context.Set<SharedWishlist>()
            .AsNoTracking()
            .Where(w => w.UserId == userId);

        var wishlists = await wishlistsQuery
            .Include(w => w.User)
            .ToListAsync(cancellationToken);

        var wishlistIdsSubquery = from w in wishlistsQuery select w.Id;
        var itemsByWishlist = await context.Set<SharedWishlistItem>()
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => wishlistIdsSubquery.Contains(i.SharedWishlistId))
            .GroupBy(i => i.SharedWishlistId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList(), cancellationToken);

        List<SharedWishlistDto> result = [];
        foreach (var wishlist in wishlists)
        {
            var baseDto = mapper.Map<SharedWishlistDto>(wishlist);
            if (itemsByWishlist.TryGetValue(wishlist.Id, out var wishlistItems))
            {
                var mappedItems = mapper.Map<List<SharedWishlistItemDto>>(wishlistItems);
                result.Add(baseDto with { Items = mappedItems, ItemCount = wishlistItems.Count });
            }
            else
            {
                result.Add(baseDto with { Items = [], ItemCount = 0 });
            }
        }

        return result;
    }

    public async Task DeleteSharedWishlistAsync(
        Guid wishlistId,
        CancellationToken cancellationToken = default)
    {
        var wishlist = await context.Set<SharedWishlist>()
            .FirstOrDefaultAsync(w => w.Id == wishlistId, cancellationToken);
        if (wishlist is not null)
        {
            wishlist.MarkAsDeleted();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkItemAsPurchasedAsync(
        Guid itemId, 
        Guid purchasedBy,
        CancellationToken cancellationToken = default)
    {
        var item = await context.Set<SharedWishlistItem>()
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is not null)
        {
            item.MarkAsPurchased(purchasedBy);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
