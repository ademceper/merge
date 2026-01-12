using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// BundleItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class BundleItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid BundleId { get; private set; }
    public Guid ProductId { get; private set; }
    
    private int _quantity = 1;
    public int Quantity 
    { 
        get => _quantity; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Quantity));
            _quantity = value;
        } 
    }
    
    private int _sortOrder = 0;
    public int SortOrder 
    { 
        get => _sortOrder; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(SortOrder));
            _sortOrder = value;
        } 
    }
    
    // Navigation properties
    public ProductBundle Bundle { get; private set; } = null!;
    public Product Product { get; private set; } = null!;
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private BundleItem() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static BundleItem Create(
        Guid bundleId,
        Guid productId,
        int quantity = 1,
        int sortOrder = 0)
    {
        Guard.AgainstDefault(bundleId, nameof(bundleId));
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNegative(sortOrder, nameof(sortOrder));
        
        var item = new BundleItem
        {
            Id = Guid.NewGuid(),
            BundleId = bundleId,
            ProductId = productId,
            _quantity = quantity,
            _sortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
        
        return item;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update quantity
    public void UpdateQuantity(int newQuantity)
    {
        Guard.AgainstNegativeOrZero(newQuantity, nameof(newQuantity));
        _quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update sort order
    public void UpdateSortOrder(int newSortOrder)
    {
        Guard.AgainstNegative(newSortOrder, nameof(newSortOrder));
        _sortOrder = newSortOrder;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Mark as deleted
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

