using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// SharedWishlistItem Entity - Rich Domain Model implementation
/// BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SharedWishlistItem : BaseEntity
{
    public Guid SharedWishlistId { get; private set; }
    public Guid ProductId { get; private set; }
    
    private int _priority = 0; // 1 = High, 2 = Medium, 3 = Low
    public int Priority 
    { 
        get => _priority; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0, 3, nameof(Priority));
            _priority = value;
        } 
    }
    
    private string _note = string.Empty;
    public string Note 
    { 
        get => _note; 
        private set 
        {
            if (value != null && value.Length > 500)
            {
                throw new DomainException("Not en fazla 500 karakter olabilir");
            }
            _note = value ?? string.Empty;
        } 
    }
    
    public bool IsPurchased { get; private set; } = false;
    public Guid? PurchasedBy { get; private set; }
    public DateTime? PurchasedAt { get; private set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public SharedWishlist SharedWishlist { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    public User? PurchasedByUser { get; private set; }
    
    private SharedWishlistItem() { }
    
    public static SharedWishlistItem Create(
        Guid sharedWishlistId,
        Guid productId,
        int priority = 0,
        string note = "")
    {
        Guard.AgainstDefault(sharedWishlistId, nameof(sharedWishlistId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstOutOfRange(priority, 0, 3, nameof(priority));
        
        if (note != null && note.Length > 500)
        {
            throw new DomainException("Not en fazla 500 karakter olabilir");
        }
        
        var item = new SharedWishlistItem
        {
            Id = Guid.NewGuid(),
            SharedWishlistId = sharedWishlistId,
            ProductId = productId,
            _priority = priority,
            _note = note ?? string.Empty,
            IsPurchased = false,
            PurchasedBy = null,
            PurchasedAt = null,
            CreatedAt = DateTime.UtcNow
        };
        
        item.ValidateInvariants();
        
        return item;
    }
    
    public void UpdatePriority(int newPriority)
    {
        Guard.AgainstOutOfRange(newPriority, 0, 3, nameof(newPriority));
        _priority = newPriority;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateNote(string newNote)
    {
        if (newNote != null && newNote.Length > 500)
        {
            throw new DomainException("Not en fazla 500 karakter olabilir");
        }
        _note = newNote ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void MarkAsPurchased(Guid purchasedBy)
    {
        Guard.AgainstDefault(purchasedBy, nameof(purchasedBy));
        
        if (IsPurchased)
            return;
        
        IsPurchased = true;
        PurchasedBy = purchasedBy;
        PurchasedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UnmarkAsPurchased()
    {
        if (!IsPurchased)
            return;
        
        IsPurchased = false;
        PurchasedBy = null;
        PurchasedAt = null;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == SharedWishlistId)
            throw new DomainException("Shared wishlist ID boş olamaz");

        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (_priority < 0 || _priority > 3)
            throw new DomainException("Öncelik 0-3 arasında olmalıdır");

        if (_note.Length > 500)
            throw new DomainException("Not en fazla 500 karakter olabilir");
    }
}

