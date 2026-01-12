using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// RecentlyViewedProduct Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class RecentlyViewedProduct : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime ViewedAt { get; private set; }

    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private RecentlyViewedProduct() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static RecentlyViewedProduct Create(Guid userId, Guid productId)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(productId, nameof(productId));

        var recentlyViewed = new RecentlyViewedProduct
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            ViewedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        // ✅ BOLUM 1.4: Invariant validation
        recentlyViewed.ValidateInvariants();
        
        return recentlyViewed;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update viewed timestamp
    public void UpdateViewedAt()
    {
        ViewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.4: Invariant validation
        ValidateInvariants();
    }

    // ✅ BOLUM 1.4: Invariant validation
    private void ValidateInvariants()
    {
        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");
    }
}

