using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// LiveStream Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

