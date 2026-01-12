using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Identity;
namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// SharedWishlist Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SharedWishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public string ShareCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

