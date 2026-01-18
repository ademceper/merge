using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// SharedWishlist Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SharedWishlist : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    
    private string _shareCode = string.Empty;
    public string ShareCode 
    { 
        get => _shareCode; 
        private set 
        {
            if (!string.IsNullOrEmpty(value) && value.Length < 6)
            {
                throw new DomainException("Share code en az 6 karakter olmalıdır");
            }
            if (!string.IsNullOrEmpty(value) && value.Length > 50)
            {
                throw new DomainException("Share code en fazla 50 karakter olabilir");
            }
            _shareCode = value;
        } 
    }
    
    private string _name = string.Empty;
    public string Name 
    { 
        get => _name; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(Name));
            if (value.Length > 200)
            {
                throw new DomainException("İsim en fazla 200 karakter olabilir");
            }
            _name = value;
        } 
    }
    
    public string Description { get; private set; } = string.Empty;
    public bool IsPublic { get; private set; } = false;
    
    private int _viewCount = 0;
    public int ViewCount 
    { 
        get => _viewCount; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(ViewCount));
            _viewCount = value;
        } 
    }
    
    public DateTime? ExpiresAt { get; private set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    private readonly List<SharedWishlistItem> _items = new();
    public IReadOnlyCollection<SharedWishlistItem> Items => _items.AsReadOnly();
    
    private SharedWishlist() { }
    
    public static SharedWishlist Create(
        Guid userId,
        string name,
        string description = "",
        bool isPublic = false,
        DateTime? expiresAt = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        
        if (name.Length > 200)
        {
            throw new DomainException("İsim en fazla 200 karakter olabilir");
        }
        
        var sharedWishlist = new SharedWishlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            _name = name,
            Description = description ?? string.Empty,
            IsPublic = isPublic,
            _viewCount = 0,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
        
        sharedWishlist.ValidateInvariants();
        
        sharedWishlist.AddDomainEvent(new SharedWishlistCreatedEvent(
            sharedWishlist.Id,
            userId,
            name,
            isPublic));
        
        return sharedWishlist;
    }
    
    public void GenerateShareCode()
    {
        // Generate a unique 8-character share code
        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        ShareCode = code;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, Name));
    }
    
    public void ClearShareCode()
    {
        _shareCode = string.Empty;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, _name));
    }
    
    public void UpdateName(string newName)
    {
        Guard.AgainstNullOrEmpty(newName, nameof(newName));
        if (newName.Length > 200)
        {
            throw new DomainException("İsim en fazla 200 karakter olabilir");
        }
        _name = newName;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, _name));
    }
    
    public void UpdateDescription(string newDescription)
    {
        Guard.AgainstNull(newDescription, nameof(newDescription));
        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, _name));
    }
    
    public void SetPublic(bool isPublic)
    {
        if (IsPublic == isPublic) return;
        
        IsPublic = isPublic;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, _name));
    }
    
    public void IncrementViewCount()
    {
        _viewCount++;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        // View count değişikliği önemli bir business event'tir
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, _name));
    }
    
    public void UpdateExpiryDate(DateTime? newExpiresAt)
    {
        ExpiresAt = newExpiresAt;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistUpdatedEvent(Id, UserId, _name));
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    }
    
    public void AddItem(SharedWishlistItem item)
    {
        Guard.AgainstNull(item, nameof(item));
        if (item.SharedWishlistId != Id)
        {
            throw new DomainException("Item bu wishlist'e ait değil");
        }
        if (_items.Any(i => i.Id == item.Id))
        {
            throw new DomainException("Bu item zaten eklenmiş");
        }
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void RemoveItem(Guid itemId)
    {
        Guard.AgainstDefault(itemId, nameof(itemId));
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item is null)
        {
            throw new DomainException("Item bulunamadı");
        }
        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
        
        AddDomainEvent(new SharedWishlistDeletedEvent(Id, UserId, _name));
    }

    private void ValidateInvariants()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new DomainException("Wishlist adı boş olamaz");

        if (_name.Length > 200)
            throw new DomainException("Wishlist adı en fazla 200 karakter olabilir");

        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (!string.IsNullOrEmpty(_shareCode) && (_shareCode.Length < 6 || _shareCode.Length > 50))
            throw new DomainException("Share code 6-50 karakter arasında olmalıdır");

        if (_viewCount < 0)
            throw new DomainException("Görüntülenme sayısı negatif olamaz");
    }
}

