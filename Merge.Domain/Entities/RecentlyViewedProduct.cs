using Merge.Domain.Common;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

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

        return new RecentlyViewedProduct
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            ViewedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Update viewed timestamp
    public void UpdateViewedAt()
    {
        ViewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

