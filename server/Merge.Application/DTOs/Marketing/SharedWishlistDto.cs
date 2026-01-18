using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;


public record SharedWishlistDto(
    Guid Id,
    string ShareCode,
    string Name,
    string Description,
    bool IsPublic,
    int ViewCount,
    int ItemCount,
    List<SharedWishlistItemDto> Items);
