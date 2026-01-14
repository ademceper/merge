using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Shared Wishlist DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record SharedWishlistDto(
    Guid Id,
    string ShareCode,
    string Name,
    string Description,
    bool IsPublic,
    int ViewCount,
    int ItemCount,
    List<SharedWishlistItemDto> Items);
