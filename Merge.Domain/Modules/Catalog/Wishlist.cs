using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// Wishlist Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class Wishlist : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Wishlist() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Wishlist Create(Guid userId, Guid productId)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(productId, nameof(productId));

        return new Wishlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

