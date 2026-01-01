using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class LiveStream : BaseEntity
{
    public Guid SellerId { get; set; }
    public SellerProfile? Seller { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public LiveStreamStatus Status { get; set; } = LiveStreamStatus.Scheduled;
    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? StreamUrl { get; set; } // RTMP/WebRTC stream URL
    public string? StreamKey { get; set; } // Stream key for broadcasting
    public string? ThumbnailUrl { get; set; }
    public int ViewerCount { get; set; } = 0;
    public int PeakViewerCount { get; set; } = 0;
    public int TotalViewerCount { get; set; } = 0;
    public int OrderCount { get; set; } = 0; // Orders created during stream
    public decimal Revenue { get; set; } = 0; // Revenue from stream orders
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; } // Stream category
    public string? Tags { get; set; } // Comma separated tags
    
    // Navigation properties
    public ICollection<LiveStreamProduct> Products { get; set; } = new List<LiveStreamProduct>();
    public ICollection<LiveStreamViewer> Viewers { get; set; } = new List<LiveStreamViewer>();
    public ICollection<LiveStreamOrder> Orders { get; set; } = new List<LiveStreamOrder>();
}

public class LiveStreamProduct : BaseEntity
{
    public Guid LiveStreamId { get; set; }
    public LiveStream LiveStream { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int DisplayOrder { get; set; } = 0;
    public bool IsHighlighted { get; set; } = false; // Currently showcased
    public DateTime? ShowcasedAt { get; set; } // When product was showcased
    public int ViewCount { get; set; } = 0; // Views during showcase
    public int ClickCount { get; set; } = 0; // Clicks during showcase
    public int OrderCount { get; set; } = 0; // Orders during showcase
    public decimal? SpecialPrice { get; set; } // Special price during stream
    public string? ShowcaseNotes { get; set; } // Notes about the product showcase
}

public class LiveStreamViewer : BaseEntity
{
    public Guid LiveStreamId { get; set; }
    public LiveStream LiveStream { get; set; } = null!;
    public Guid? UserId { get; set; } // Nullable for anonymous viewers
    public User? User { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public int WatchDuration { get; set; } = 0; // In seconds
    public bool IsActive { get; set; } = true; // Currently watching
    public string? GuestId { get; set; } // For anonymous viewers
}

public class LiveStreamOrder : BaseEntity
{
    public Guid LiveStreamId { get; set; }
    public LiveStream LiveStream { get; set; } = null!;
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? ProductId { get; set; } // Product that triggered the order
    public Product? Product { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal OrderAmount { get; set; }
}

public class PageBuilder : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON content for page builder
    public string? Template { get; set; } // Template identifier
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public Guid? AuthorId { get; set; }
    public User? Author { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public int ViewCount { get; set; } = 0;
    public string? PageType { get; set; } // Home, Category, Product, Custom
    public Guid? RelatedEntityId { get; set; } // Related category/product ID if applicable
}

