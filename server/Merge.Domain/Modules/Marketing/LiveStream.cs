using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
    public Guid SellerId { get; private set; }
    public SellerProfile? Seller { get; private set; }
    
    private string _title = string.Empty;
    public string Title 
    { 
        get => _title; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Title));
            Guard.AgainstLength(value, 200, nameof(Title));
            if (value.Length < 2)
                throw new ArgumentException("Title must be at least 2 characters", nameof(Title));
            _title = value;
        } 
    }
    
    private string _description = string.Empty;
    public string Description 
    { 
        get => _description; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, 2000, nameof(Description));
            }
            _description = value ?? string.Empty;
        } 
    }
    public LiveStreamStatus Status { get; private set; } = LiveStreamStatus.Scheduled;
    public DateTime? ScheduledStartTime { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    
    private string? _streamUrl;
    public string? StreamUrl 
    { 
        get => _streamUrl; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (!IsValidUrl(value))
                    throw new DomainException("Geçerli bir Stream URL giriniz.");
                Guard.AgainstLength(value, 500, nameof(StreamUrl));
            }
            _streamUrl = value;
        } 
    }
    
    private string? _streamKey;
    public string? StreamKey 
    { 
        get => _streamKey; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, 200, nameof(StreamKey));
            }
            _streamKey = value;
        } 
    }
    
    private string? _thumbnailUrl;
    public string? ThumbnailUrl 
    { 
        get => _thumbnailUrl; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (!IsValidUrl(value))
                    throw new DomainException("Geçerli bir Thumbnail URL giriniz.");
                Guard.AgainstLength(value, 500, nameof(ThumbnailUrl));
            }
            _thumbnailUrl = value;
        } 
    }
    
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
    
    [NotMapped]
    public Money RevenueMoney => new Money(_revenue);
    
    public bool IsActive { get; private set; } = true;
    
    private string? _category;
    public string? Category 
    { 
        get => _category; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, 100, nameof(Category));
            }
            _category = value;
        } 
    }
    
    private string? _tags;
    public string? Tags 
    { 
        get => _tags; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, 500, nameof(Tags));
            }
            _tags = value;
        } 
    }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    private readonly List<LiveStreamProduct> _products = [];
    private readonly List<LiveStreamViewer> _viewers = [];
    private readonly List<LiveStreamOrder> _orders = [];
    
    public IReadOnlyCollection<LiveStreamProduct> Products => _products.AsReadOnly();
    public IReadOnlyCollection<LiveStreamViewer> Viewers => _viewers.AsReadOnly();
    public IReadOnlyCollection<LiveStreamOrder> Orders => _orders.AsReadOnly();

    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı (Organization, Team entity'lerinde de aynı pattern kullanılıyor)
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    private LiveStream() { }

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
            _title = title,
            _description = description ?? string.Empty,
            Status = LiveStreamStatus.Scheduled,
            ScheduledStartTime = scheduledStartTime,
            _streamUrl = streamUrl,
            _streamKey = streamKey,
            _thumbnailUrl = thumbnailUrl,
            _category = category,
            _tags = tags,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        stream.ValidateInvariants();

        stream.AddDomainEvent(new LiveStreamCreatedEvent(stream.Id, stream.SellerId, stream.Title, stream.ScheduledStartTime));

        return stream;
    }

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
        Description = description ?? string.Empty;
        ScheduledStartTime = scheduledStartTime;
        StreamUrl = streamUrl;
        StreamKey = streamKey;
        ThumbnailUrl = thumbnailUrl;
        Category = category;
        Tags = tags;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamUpdatedEvent(Id, SellerId, Title, UpdatedAt.Value));
    }

    public void Start()
    {
        if (Status != LiveStreamStatus.Scheduled && Status != LiveStreamStatus.Paused)
            throw new DomainException("Sadece planlanmış veya duraklatılmış yayınlar başlatılabilir.");

        Status = LiveStreamStatus.Live;
        ActualStartTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamStartedEvent(Id, SellerId, ActualStartTime.Value));
    }

    public void End()
    {
        if (Status != LiveStreamStatus.Live && Status != LiveStreamStatus.Paused)
            throw new DomainException("Sadece canlı veya duraklatılmış yayınlar sonlandırılabilir.");

        Status = LiveStreamStatus.Ended;
        EndTime = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamEndedEvent(Id, SellerId, EndTime.Value, TotalViewerCount, OrderCount, Revenue));
    }

    public void Pause()
    {
        if (Status != LiveStreamStatus.Live)
            throw new DomainException("Sadece canlı yayınlar duraklatılabilir.");

        Status = LiveStreamStatus.Paused;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamPausedEvent(Id, SellerId, UpdatedAt.Value));
    }

    public void Resume()
    {
        if (Status != LiveStreamStatus.Paused)
            throw new DomainException("Sadece duraklatılmış yayınlar devam ettirilebilir.");

        Status = LiveStreamStatus.Live;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamResumedEvent(Id, SellerId, UpdatedAt.Value));
    }

    public void Cancel()
    {
        if (Status == LiveStreamStatus.Live)
            throw new DomainException("Canlı yayınlar iptal edilemez, önce sonlandırılmalıdır.");

        if (Status == LiveStreamStatus.Ended)
            throw new DomainException("Zaten sonlandırılmış yayınlar iptal edilemez.");

        Status = LiveStreamStatus.Cancelled;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamCancelledEvent(Id, SellerId, UpdatedAt.Value));
    }

    public void IncrementViewerCount()
    {
        ViewerCount++;
        TotalViewerCount++;
        if (ViewerCount > PeakViewerCount)
        {
            PeakViewerCount = ViewerCount;
        }
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void DecrementViewerCount()
    {
        if (ViewerCount > 0)
        {
            ViewerCount--;
        }
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void AddProduct(LiveStreamProduct product)
    {
        Guard.AgainstNull(product, nameof(product));
        
        if (product.LiveStreamId != Id)
            throw new DomainException("Product must belong to this stream");

        // Check if product already exists (not deleted)
        if (_products.Any(p => p.ProductId == product.ProductId && !p.IsDeleted))
            throw new DomainException("Product is already added to this stream");

        _products.Add(product);
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void RemoveProduct(Guid productId)
    {
        Guard.AgainstDefault(productId, nameof(productId));

        var product = _products.FirstOrDefault(p => p.ProductId == productId && !p.IsDeleted);
        if (product == null)
            throw new DomainException("Product not found in this stream");

        product.MarkAsDeleted();
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void AddViewer(LiveStreamViewer viewer)
    {
        Guard.AgainstNull(viewer, nameof(viewer));
        
        if (viewer.LiveStreamId != Id)
            throw new DomainException("Viewer must belong to this stream");

        if (Status != LiveStreamStatus.Live)
            throw new DomainException("Viewers can only join live streams");

        _viewers.Add(viewer);
        IncrementViewerCount();
        
        ValidateInvariants();
    }

    public void RemoveViewer(Guid viewerId)
    {
        Guard.AgainstDefault(viewerId, nameof(viewerId));

        var viewer = _viewers.FirstOrDefault(v => v.Id == viewerId && !v.IsDeleted);
        if (viewer == null)
            throw new DomainException("Viewer not found in this stream");

        viewer.Leave();
        DecrementViewerCount();
        
        ValidateInvariants();
    }

    public void AddOrder(LiveStreamOrder order)
    {
        Guard.AgainstNull(order, nameof(order));
        
        if (order.LiveStreamId != Id)
            throw new DomainException("Order must belong to this stream");

        _orders.Add(order);
        OrderCount++;
        Revenue += order.OrderAmount;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void AddOrder(decimal orderAmount)
    {
        Guard.AgainstNegativeOrZero(orderAmount, nameof(orderAmount));

        OrderCount++;
        Revenue += orderAmount;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;

        if (Status == LiveStreamStatus.Live)
            throw new DomainException("Canlı yayınlar silinemez, önce sonlandırılmalıdır.");

        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamDeletedEvent(Id, SellerId, Title, UpdatedAt.Value));
    }

    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        ValidateInvariants();

        AddDomainEvent(new LiveStreamRestoredEvent(Id, SellerId, Title, UpdatedAt.Value));
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(Title))
            throw new DomainException("Başlık boş olamaz");

        if (Title.Length < 2 || Title.Length > 200)
            throw new DomainException("Başlık 2-200 karakter arasında olmalıdır");

        if (!string.IsNullOrEmpty(Description) && Description.Length > 2000)
            throw new DomainException("Açıklama en fazla 2000 karakter olabilir");

        if (Guid.Empty == SellerId)
            throw new DomainException("Satıcı ID boş olamaz");

        if (Revenue < 0)
            throw new DomainException("Gelir negatif olamaz");

        if (OrderCount < 0)
            throw new DomainException("Sipariş sayısı negatif olamaz");

        if (ViewerCount < 0)
            throw new DomainException("İzleyici sayısı negatif olamaz");

        if (TotalViewerCount < 0)
            throw new DomainException("Toplam izleyici sayısı negatif olamaz");

        if (PeakViewerCount < 0)
            throw new DomainException("Zirve izleyici sayısı negatif olamaz");

        if (ScheduledStartTime.HasValue && EndTime.HasValue && EndTime.Value < ScheduledStartTime.Value)
            throw new DomainException("Bitiş zamanı başlangıç zamanından önce olamaz");

        if (ActualStartTime.HasValue && EndTime.HasValue && EndTime.Value < ActualStartTime.Value)
            throw new DomainException("Bitiş zamanı gerçek başlangıç zamanından önce olamaz");
    }
}

