namespace Merge.Domain.Entities;

/// <summary>
/// LiveStreamViewer Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

