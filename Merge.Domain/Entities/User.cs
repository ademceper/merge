using Microsoft.AspNetCore.Identity;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// User Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// NOT: IdentityUser'dan türüyor, bu yüzden BaseEntity'den türemiyor. Domain event'ler manuel olarak ekleniyor.
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class User : IdentityUser<Guid>, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation (mümkün olduğunca)
    // NOT: IdentityUser base class'ı bazı property'leri public set gerektiriyor, bu yüzden kısmi private set kullanıyoruz
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public ICollection<Address> Addresses { get; private set; } = new List<Address>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    public ICollection<Cart> Carts { get; private set; } = new List<Cart>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<Wishlist> Wishlists { get; private set; } = new List<Wishlist>();
    public ICollection<CouponUsage> CouponUsages { get; private set; } = new List<CouponUsage>();
    public ICollection<ReturnRequest> ReturnRequests { get; private set; } = new List<ReturnRequest>();
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
    public ICollection<RecentlyViewedProduct> RecentlyViewedProducts { get; private set; } = new List<RecentlyViewedProduct>();
    public SellerProfile? SellerProfile { get; private set; }
    public ICollection<SavedCartItem> SavedCartItems { get; private set; } = new List<SavedCartItem>();
    public ICollection<EmailVerification> EmailVerifications { get; private set; } = new List<EmailVerification>();
    public ICollection<GiftCard> PurchasedGiftCards { get; private set; } = new List<GiftCard>();
    public ICollection<GiftCard> AssignedGiftCards { get; private set; } = new List<GiftCard>();
    
    // Organization & Team
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; private set; }
    public ICollection<TeamMember> TeamMemberships { get; private set; } = new List<TeamMember>();

    // ✅ SECURITY: Refresh tokens for JWT authentication
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // ✅ BOLUM 1.5: Domain Events - Add domain event
    // NOT: Public yapıldı çünkü command handler'lardan erişilmesi gerekiyor
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Add(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Clear domain events
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    // ✅ BOLUM 1.1: Domain Method - Update user profile
    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null)
    {
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        if (phoneNumber != null)
        {
            PhoneNumber = phoneNumber;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("User is already deleted");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Restore user
    public void Restore()
    {
        if (!IsDeleted)
            throw new DomainException("User is not deleted");

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Confirm email
    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            throw new DomainException("Email is already confirmed");

        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

