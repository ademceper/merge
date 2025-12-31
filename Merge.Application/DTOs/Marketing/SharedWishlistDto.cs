namespace Merge.Application.DTOs.Marketing;

public class SharedWishlistDto
{
    public Guid Id { get; set; }
    public string ShareCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public int ViewCount { get; set; }
    public int ItemCount { get; set; }
    public List<SharedWishlistItemDto> Items { get; set; } = new();
}
