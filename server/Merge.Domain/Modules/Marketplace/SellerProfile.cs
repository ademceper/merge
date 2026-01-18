using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerProfile Entity - Rich Domain Model implementation
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerProfile : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public string StoreName { get; private set; } = string.Empty;
    public string? StoreDescription { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public SellerStatus Status { get; private set; } = SellerStatus.Pending;
    public decimal CommissionRate { get; private set; } = 0; // Yüzde olarak
    public decimal TotalEarnings { get; private set; } = 0;
    public decimal PendingBalance { get; private set; } = 0;
    public decimal AvailableBalance { get; private set; } = 0;
    public int TotalOrders { get; private set; } = 0;
    public int TotalProducts { get; private set; } = 0;
    public decimal AverageRating { get; private set; } = 0;
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationNotes { get; private set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;

    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    private SellerProfile() { }

    public static SellerProfile Create(
        Guid userId,
        string storeName,
        string? storeDescription = null,
        string? logoUrl = null,
        string? bannerUrl = null,
        decimal commissionRate = 0)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(storeName, nameof(storeName));
        Guard.AgainstNegative(commissionRate, nameof(commissionRate));

        var profile = new SellerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StoreName = storeName,
            StoreDescription = storeDescription,
            LogoUrl = logoUrl,
            BannerUrl = bannerUrl,
            CommissionRate = commissionRate,
            Status = SellerStatus.Pending,
            TotalEarnings = 0,
            PendingBalance = 0,
            AvailableBalance = 0,
            TotalOrders = 0,
            TotalProducts = 0,
            AverageRating = 0,
            CreatedAt = DateTime.UtcNow
        };

        profile.AddDomainEvent(new SellerProfileCreatedEvent(profile.Id, userId, storeName));

        return profile;
    }

    public void UpdateStoreDetails(
        string? storeName = null,
        string? storeDescription = null,
        string? logoUrl = null,
        string? bannerUrl = null)
    {
        if (storeName != null)
        {
            Guard.AgainstNullOrEmpty(storeName, nameof(storeName));
            StoreName = storeName;
        }

        if (storeDescription != null)
            StoreDescription = storeDescription;

        if (logoUrl != null)
            LogoUrl = logoUrl;

        if (bannerUrl != null)
            BannerUrl = bannerUrl;

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCommissionRate(decimal commissionRate)
    {
        Guard.AgainstNegative(commissionRate, nameof(commissionRate));

        CommissionRate = commissionRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Verify(string? verificationNotes = null)
    {
        if (IsVerified())
            throw new DomainException("Satıcı zaten doğrulanmış");

        VerifiedAt = DateTime.UtcNow;
        VerificationNotes = verificationNotes;
        Status = SellerStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerProfileVerifiedEvent(Id, UserId));
    }

    public void Activate()
    {
        if (Status == SellerStatus.Active)
            throw new DomainException("Satıcı zaten aktif");

        Status = SellerStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerProfileActivatedEvent(Id, UserId));
    }

    public void Suspend()
    {
        if (Status == SellerStatus.Suspended)
            throw new DomainException("Satıcı zaten askıya alınmış");

        Status = SellerStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new SellerProfileSuspendedEvent(Id, UserId));
    }

    public void AddEarnings(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        TotalEarnings += amount;
        PendingBalance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MoveBalanceToAvailable(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        if (PendingBalance < amount)
            throw new DomainException("Yetersiz bekleyen bakiye");

        PendingBalance -= amount;
        AvailableBalance += amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductFromAvailableBalance(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        if (AvailableBalance < amount)
            throw new DomainException("Yetersiz kullanılabilir bakiye");

        AvailableBalance -= amount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementOrderCount()
    {
        TotalOrders++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementProductCount()
    {
        TotalProducts++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAverageRating(decimal averageRating)
    {
        Guard.AgainstOutOfRange(averageRating, 0, 5, nameof(averageRating));

        AverageRating = averageRating;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsVerified() => VerifiedAt.HasValue;
}
