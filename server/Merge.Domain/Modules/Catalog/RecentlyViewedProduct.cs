using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Catalog;


public class RecentlyViewedProduct : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTime ViewedAt { get; private set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User User { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    private RecentlyViewedProduct() { }

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
        
        recentlyViewed.ValidateInvariants();
        
        return recentlyViewed;
    }

    public void UpdateViewedAt()
    {
        ViewedAt = DateTime.UtcNow;
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
        if (Guid.Empty == UserId)
            throw new DomainException("Kullanıcı ID boş olamaz");

        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");
    }
}

