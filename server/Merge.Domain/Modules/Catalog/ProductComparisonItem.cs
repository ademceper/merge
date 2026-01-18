using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductComparisonItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ProductComparisonItem : BaseEntity
{
    public Guid ComparisonId { get; private set; }
    public ProductComparison Comparison { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    private int _position = 0;
    public int Position 
    { 
        get => _position; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(Position));
            _position = value;
        } 
    }
    
    public DateTime AddedAt { get; private set; } = DateTime.UtcNow;
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    private ProductComparisonItem() { }
    
    public static ProductComparisonItem Create(
        Guid comparisonId,
        Guid productId,
        int position = 0)
    {
        Guard.AgainstDefault(comparisonId, nameof(comparisonId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegative(position, nameof(position));
        
        var item = new ProductComparisonItem
        {
            Id = Guid.NewGuid(),
            ComparisonId = comparisonId,
            ProductId = productId,
            _position = position,
            AddedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        
        item.ValidateInvariants();
        
        return item;
    }
    
    public void UpdatePosition(int newPosition)
    {
        Guard.AgainstNegative(newPosition, nameof(newPosition));
        _position = newPosition;
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
        if (Guid.Empty == ComparisonId)
            throw new DomainException("Karşılaştırma ID boş olamaz");

        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (_position < 0)
            throw new DomainException("Pozisyon negatif olamaz");
    }
}

