using Merge.Domain.SharedKernel;
using Microsoft.AspNetCore.Identity;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Modules.Identity;

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
    private readonly List<IDomainEvent> _domainEvents = [];

    // NOT: IdentityUser base class'ı bazı property'leri public set gerektiriyor, bu yüzden kısmi private set kullanıyoruz
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;
    
    // Navigation properties
    public ICollection<Address> Addresses { get; private set; } = [];
    public ICollection<Order> Orders { get; private set; } = [];
    public ICollection<Cart> Carts { get; private set; } = [];
    public ICollection<Review> Reviews { get; private set; } = [];
    public ICollection<Wishlist> Wishlists { get; private set; } = [];
    public ICollection<CouponUsage> CouponUsages { get; private set; } = [];
    public ICollection<ReturnRequest> ReturnRequests { get; private set; } = [];
    public ICollection<Notification> Notifications { get; private set; } = [];
    public ICollection<RecentlyViewedProduct> RecentlyViewedProducts { get; private set; } = [];
    public SellerProfile? SellerProfile { get; private set; }
    public ICollection<SavedCartItem> SavedCartItems { get; private set; } = [];
    public ICollection<EmailVerification> EmailVerifications { get; private set; } = [];
    public ICollection<GiftCard> PurchasedGiftCards { get; private set; } = [];
    public ICollection<GiftCard> AssignedGiftCards { get; private set; } = [];
    
    // Organization & Team
    public Guid? OrganizationId { get; private set; }
    public Organization? Organization { get; private set; }
    public ICollection<TeamMember> TeamMemberships { get; private set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static User Create(string firstName, string lastName, string email, string? phoneNumber = null)
    {
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstLength(firstName, 100, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstLength(lastName, 100, nameof(lastName));
        Guard.AgainstNullOrEmpty(email, nameof(email));

        var emailValueObject = new Email(email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = emailValueObject.Value,
            UserName = emailValueObject.Value,
            PhoneNumber = phoneNumber,
            SecurityStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        user.AddDomainEvent(new UserCreatedEvent(user.Id, firstName, lastName, emailValueObject.Value));
        
        return user;
    }

    // NOT: Public yapıldı çünkü command handler'lardan erişilmesi gerekiyor
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        _domainEvents.Remove(domainEvent);
    }

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber = null)
    {
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstLength(firstName, 100, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));
        Guard.AgainstLength(lastName, 100, nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        if (phoneNumber != null)
        {
            PhoneNumber = phoneNumber;
        }
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserUpdatedEvent(Id, FirstName, LastName, PhoneNumber));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException("User is already deleted");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeletedEvent(Id));
    }

    public void Restore()
    {
        if (!IsDeleted)
            throw new DomainException("User is not deleted");

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserRestoredEvent(Id));
    }

    public void ConfirmEmail()
    {
        if (EmailConfirmed)
            throw new DomainException("Email is already confirmed");

        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserEmailConfirmedEvent(Id, Email ?? string.Empty));
    }

    public void Activate()
    {
        if (EmailConfirmed)
            throw new DomainException("User is already activated");

        EmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserActivatedEvent(Id, Email ?? string.Empty));
    }

    public void Deactivate()
    {
        if (!EmailConfirmed)
            throw new DomainException("User is already deactivated");

        EmailConfirmed = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedEvent(Id, Email ?? string.Empty));
    }
}

