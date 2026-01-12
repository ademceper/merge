using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductSizeGuide Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ProductSizeGuide : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid SizeGuideId { get; private set; }
    public SizeGuide SizeGuide { get; private set; } = null!;
    
    public string? CustomNotes { get; private set; } // Product-specific sizing notes
    public bool FitType { get; private set; } = true; // true = Regular Fit, false = Slim Fit, etc.
    
    private string? _fitDescription;
    public string? FitDescription 
    { 
        get => _fitDescription; 
        private set 
        {
            if (!string.IsNullOrEmpty(value) && value.Length > 500)
            {
                throw new DomainException("Fit açıklaması en fazla 500 karakter olabilir");
            }
            _fitDescription = value;
        } 
    }
    
    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private ProductSizeGuide() { }
    
    // ✅ BOLUM 1.1: Factory Method with validation
    public static ProductSizeGuide Create(
        Guid productId,
        Guid sizeGuideId,
        string? customNotes = null,
        bool fitType = true,
        string? fitDescription = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(sizeGuideId, nameof(sizeGuideId));
        
        if (!string.IsNullOrEmpty(fitDescription) && fitDescription.Length > 500)
        {
            throw new DomainException("Fit açıklaması en fazla 500 karakter olabilir");
        }
        
        var productSizeGuide = new ProductSizeGuide
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SizeGuideId = sizeGuideId,
            CustomNotes = customNotes,
            FitType = fitType,
            _fitDescription = fitDescription,
            CreatedAt = DateTime.UtcNow
        };
        
        return productSizeGuide;
    }
    
    // ✅ BOLUM 1.1: Domain Logic - Update
    public void Update(
        Guid? sizeGuideId = null,
        string? customNotes = null,
        bool? fitType = null,
        string? fitDescription = null)
    {
        if (sizeGuideId.HasValue)
        {
            Guard.AgainstDefault(sizeGuideId.Value, nameof(sizeGuideId));
            SizeGuideId = sizeGuideId.Value;
        }
        
        if (customNotes != null) CustomNotes = customNotes;
        if (fitType.HasValue) FitType = fitType.Value;
        if (fitDescription != null) FitDescription = fitDescription;
        
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

