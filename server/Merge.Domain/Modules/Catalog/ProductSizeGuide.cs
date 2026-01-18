using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ProductSizeGuide Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class ProductSizeGuide : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid SizeGuideId { get; private set; }
    public SizeGuide SizeGuide { get; private set; } = null!;
    
    private string? _customNotes;
    public string? CustomNotes 
    { 
        get => _customNotes; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, ValidationConstants.MaxCustomNotesLength, nameof(CustomNotes));
            }
            _customNotes = value;
        } 
    }
    
    public bool FitType { get; private set; } = true; // true = Regular Fit, false = Slim Fit, etc.
    
    private static class ValidationConstants
    {
        public const int MaxFitDescriptionLength = 500;
        public const int MaxCustomNotesLength = 1000;
    }

    private string? _fitDescription;
    public string? FitDescription 
    { 
        get => _fitDescription; 
        private set 
        {
            if (!string.IsNullOrEmpty(value))
            {
                Guard.AgainstLength(value, ValidationConstants.MaxFitDescriptionLength, nameof(FitDescription));
            }
            _fitDescription = value;
        } 
    }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    private ProductSizeGuide() { }
    
    public static ProductSizeGuide Create(
        Guid productId,
        Guid sizeGuideId,
        string? customNotes = null,
        bool fitType = true,
        string? fitDescription = null)
    {
        Guard.AgainstDefault(productId, nameof(productId));
        Guard.AgainstDefault(sizeGuideId, nameof(sizeGuideId));
        
        if (!string.IsNullOrEmpty(fitDescription))
        {
            Guard.AgainstLength(fitDescription, ValidationConstants.MaxFitDescriptionLength, nameof(fitDescription));
        }
        
        if (!string.IsNullOrEmpty(customNotes))
        {
            Guard.AgainstLength(customNotes, ValidationConstants.MaxCustomNotesLength, nameof(customNotes));
        }
        
        var productSizeGuide = new ProductSizeGuide
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            SizeGuideId = sizeGuideId,
            _customNotes = customNotes,
            FitType = fitType,
            _fitDescription = fitDescription,
            CreatedAt = DateTime.UtcNow
        };
        
        productSizeGuide.ValidateInvariants();
        
        return productSizeGuide;
    }
    
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
        
        if (customNotes != null)
        {
            Guard.AgainstLength(customNotes, ValidationConstants.MaxCustomNotesLength, nameof(customNotes));
            CustomNotes = customNotes;
        }
        if (fitType.HasValue) FitType = fitType.Value;
        if (fitDescription != null) FitDescription = fitDescription;
        
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
        if (Guid.Empty == ProductId)
            throw new DomainException("Ürün ID boş olamaz");

        if (Guid.Empty == SizeGuideId)
            throw new DomainException("Size guide ID boş olamaz");

        if (!string.IsNullOrEmpty(_fitDescription))
        {
            Guard.AgainstLength(_fitDescription, ValidationConstants.MaxFitDescriptionLength, nameof(FitDescription));
        }

        if (!string.IsNullOrEmpty(_customNotes))
        {
            Guard.AgainstLength(_customNotes, ValidationConstants.MaxCustomNotesLength, nameof(CustomNotes));
        }
    }
}

