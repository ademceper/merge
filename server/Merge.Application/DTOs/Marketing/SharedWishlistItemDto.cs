using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;


public record SharedWishlistItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal Price,
    string ImageUrl,
    int Priority,
    string Note,
    bool IsPurchased);
