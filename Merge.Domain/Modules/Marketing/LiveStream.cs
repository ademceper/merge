using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Marketing;

/// <summary>
/// LiveStream Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class LiveStream : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid SellerId { get; private set; }
    public SellerProfile? Seller { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public LiveStreamStatus Status { get; private set; } = LiveStreamStatus.Scheduled;
    public DateTime? ScheduledStartTime { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public string? StreamUrl { get; private set; } // RTMP/WebRTC stream URL
    public string? StreamKey { get; private set; } // Stream key for broadcasting
    public string? ThumbnailUrl { get; private set; }
    
    private int _viewerCount = 0;
    public int ViewerCount 
    { 
        get => _viewerCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(ViewerCount));
            _viewerCount = value;
        } 
    }
    
    private int _peakViewerCount = 0;
    public int PeakViewerCount 
    { 
        get => _peakViewerCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(PeakViewerCount));
            _peakViewerCount = value;
        } 
    }
    
    private int _totalViewerCount = 0;
    public int TotalViewerCount 
    { 
        get => _totalViewerCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(TotalViewerCount));
            _totalViewerCount = value;
        } 
    }
    
    private int _orderCount = 0;
    public int OrderCount 
    { 
        get => _orderCount; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(OrderCount));
            _orderCount = value;
        } 
    }
    
    private decimal _revenue = 0;
    public decimal Revenue 
    { 
        get => _revenue; 
        private set 
        { 
            Guard.AgainstNegative(value, nameof(Revenue));
            _revenue = value;
        } 
    }
    
    public bool IsActive { get; private set; } = true;
    public string? Category { get; private set; } // Stream category
    public string? Tags { get; private set; } // Comma separated tags
    
    // Navigation properties
    public ICollection<LiveStreamProduct> Products { get; private set; } = new List<LiveStreamProduct>();
    public ICollection<LiveStreamViewer> Viewers { get; private set; } = new List<LiveStreamViewer>();
    public ICollection<LiveStreamOrder> Orders { get; private set; } = new List<LiveStreamOrder>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private LiveStream() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static LiveStream Create(
        Guid sellerId,
        string title,
        string description = "",
        DateTime? scheduledStartTime = null,
        string? streamUrl = null,
        string? streamKey = null,
        string? thumbnailUrl = null,
        string? category = null,
        string? tags = null)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstNullOrEmpty(title, nameof(title));

        var stream = new LiveStream
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            Title = title,
            Description = description,
            Status = LiveStreamStatus.Scheduled,
            ScheduledStartTime = scheduledStartTime,
            StreamUrl = streamUrl,
            StreamKey = streamKey,
            ThumbnailUrl = thumbnailUrl,
            Category = category,
            Tags = tags,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - LiveStreamCreatedEvent
        stream.AddDomainEvent(new LiveStreamCreatedEvent(stream.Id, stream.SellerId, stream.Title, stream.ScheduledStartTime));

        return stream;
    }

    // ✅ BOLUM 1.1: Domain Method - Update stream details
    public void UpdateDetails(
        string title,
        string description = "",
        DateTime? scheduledStartTime = null,
        string? streamUrl = null,
        string? streamKey = null,
        string? thumbnailUrl = null,
        string? category = null,
        string? tags = null)
    {
        Guard.AgainstNullOrEmpty(title, nameof(title));

        if (Status == LiveStreamStatus.Live)
            throw new DomainException("Canlı yayın sırasında detaylar güncellenemez.");

        Title = title;
        Description = description;
        ScheduledStartTime = scheduledStartTime;
        StreamUrl = streamUrl;
        StreamKey = streamKey;
        ThumbnailUrl = thumbnailUrl;
        Category = category;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Start stream
    public void Start()
    {
        if (Status != LiveStreamStatus.Scheduled && Status != LiveStreamStatus.Paused)
            throw new DomainException("Sadece planlanmış veya duraklatılmış yayınlar başlatılabilir.");

        Status = LiveStreamStatus.Live;
        ActualStartTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - LiveStreamStartedEvent
        AddDomainEvent(new LiveStreamStartedEvent(Id, SellerId, ActualStartTime.Value));
    }

    // ✅ BOLUM 1.1: Domain Method - End stream
    public void End()
    {
        if (Status != LiveStreamStatus.Live && Status != LiveStreamStatus.Paused)
            throw new DomainException("Sadece canlı veya duraklatılmış yayınlar sonlandırılabilir.");

        Status = LiveStreamStatus.Ended;
        EndTime = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - LiveStreamEndedEvent
        AddDomainEvent(new LiveStreamEndedEvent(Id, SellerId, EndTime.Value, TotalViewerCount, OrderCount, Revenue));
    }

    // ✅ BOLUM 1.1: Domain Method - Pause stream
    public void Pause()
    {
        if (Status != LiveStreamStatus.Live)
            throw new DomainException("Sadece canlı yayınlar duraklatılabilir.");

        Status = LiveStreamStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Resume stream
    public void Resume()
    {
        if (Status != LiveStreamStatus.Paused)
            throw new DomainException("Sadece duraklatılmış yayınlar devam ettirilebilir.");

        Status = LiveStreamStatus.Live;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Cancel stream
    public void Cancel()
    {
        if (Status == LiveStreamStatus.Live)
            throw new DomainException("Canlı yayınlar iptal edilemez, önce sonlandırılmalıdır.");

        if (Status == LiveStreamStatus.Ended)
            throw new DomainException("Zaten sonlandırılmış yayınlar iptal edilemez.");

        Status = LiveStreamStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Increment viewer count
    public void IncrementViewerCount()
    {
        ViewerCount++;
        TotalViewerCount++;
        if (ViewerCount > PeakViewerCount)
        {
            PeakViewerCount = ViewerCount;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Decrement viewer count
    public void DecrementViewerCount()
    {
        if (ViewerCount > 0)
        {
            ViewerCount--;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Add order
    public void AddOrder(decimal orderAmount)
    {
        Guard.AgainstNegativeOrZero(orderAmount, nameof(orderAmount));

        OrderCount++;
        Revenue += orderAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        if (Status == LiveStreamStatus.Live)
            throw new DomainException("Canlı yayınlar silinemez, önce sonlandırılmalıdır.");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

