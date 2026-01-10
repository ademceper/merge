namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Shared Wishlist Item DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record SharedWishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Price,
    string ImageUrl,
    int Priority,
    string Note,
    bool IsPurchased);
