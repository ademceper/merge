using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LiveStreamViewer Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStreamViewer : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid LiveStreamId { get; private set; }
    public LiveStream LiveStream { get; private set; } = null!;
    public Guid? UserId { get; private set; } // Nullable for anonymous viewers
    public User? User { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    
    private int _watchDuration = 0; // In seconds
    public int WatchDuration 
    { 
        get => _watchDuration; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(WatchDuration));
            _watchDuration = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true; // Currently watching
    public string? GuestId { get; private set; } // For anonymous viewers

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LiveStreamViewer() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LiveStreamViewer Create(
        Guid liveStreamId,
        Guid? userId = null,
        string? guestId = null)
    {
        Guard.AgainstDefault(liveStreamId, nameof(liveStreamId));

        if (!userId.HasValue && string.IsNullOrWhiteSpace(guestId))
            throw new DomainException("UserId veya GuestId gereklidir.");

        return new LiveStreamViewer
        {
            Id = Guid.NewGuid(),
            LiveStreamId = liveStreamId,
            UserId = userId,
            GuestId = guestId,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Method - Leave stream
    public void Leave()
    {
        if (!IsActive) return;

        LeftAt = DateTime.UtcNow;
        IsActive = false;
        WatchDuration = (int)(LeftAt.Value - JoinedAt).TotalSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update watch duration
    public void UpdateWatchDuration(int durationInSeconds)
    {
        Guard.AgainstNegative(durationInSeconds, nameof(durationInSeconds));

        _watchDuration = durationInSeconds;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        if (!LeftAt.HasValue)
        {
            LeftAt = DateTime.UtcNow;
            WatchDuration = (int)(LeftAt.Value - JoinedAt).TotalSeconds;
        }
        UpdatedAt = DateTime.UtcNow;
    }
}

