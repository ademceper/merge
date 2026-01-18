using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// SizeGuideEntry Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SizeGuideEntry : BaseEntity
{
    public Guid SizeGuideId { get; private set; }
    public SizeGuide SizeGuide { get; private set; } = null!;
    
    private string _sizeLabel = string.Empty;
    public string SizeLabel 
    { 
        get => _sizeLabel; 
        private set 
        {
            Guard.AgainstNullOrEmpty(value, nameof(SizeLabel));
            if (value.Length > 50)
            {
                throw new DomainException("Beden etiketi en fazla 50 karakter olabilir");
            }
            _sizeLabel = value;
        } 
    }
    
    public string? AlternativeLabel { get; private set; } // US 8, EU 38, UK 10
    public decimal? Chest { get; private set; }
    public decimal? Waist { get; private set; }
    public decimal? Hips { get; private set; }
    public decimal? Inseam { get; private set; }
    public decimal? Shoulder { get; private set; }
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; } // For height-based sizing
    public decimal? Weight { get; private set; } // For weight-based sizing
    public string? AdditionalMeasurements { get; private set; } // JSON for custom measurements
    
    private int _displayOrder = 0;
    public int DisplayOrder 
    { 
        get => _displayOrder; 
        private set 
        {
            Guard.AgainstNegative(value, nameof(DisplayOrder));
            _displayOrder = value;
        } 
    }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    private SizeGuideEntry() { }
    
    public static SizeGuideEntry Create(
        Guid sizeGuideId,
        string sizeLabel,
        string? alternativeLabel = null,
        decimal? chest = null,
        decimal? waist = null,
        decimal? hips = null,
        decimal? inseam = null,
        decimal? shoulder = null,
        decimal? length = null,
        decimal? width = null,
        decimal? height = null,
        decimal? weight = null,
        string? additionalMeasurements = null,
        int displayOrder = 0)
    {
        Guard.AgainstDefault(sizeGuideId, nameof(sizeGuideId));
        Guard.AgainstNullOrEmpty(sizeLabel, nameof(sizeLabel));
        Guard.AgainstNegative(displayOrder, nameof(displayOrder));
        
        if (sizeLabel.Length > 50)
        {
            throw new DomainException("Beden etiketi en fazla 50 karakter olabilir");
        }
        
        var entry = new SizeGuideEntry
        {
            Id = Guid.NewGuid(),
            SizeGuideId = sizeGuideId,
            _sizeLabel = sizeLabel,
            AlternativeLabel = alternativeLabel,
            Chest = chest,
            Waist = waist,
            Hips = hips,
            Inseam = inseam,
            Shoulder = shoulder,
            Length = length,
            Width = width,
            Height = height,
            Weight = weight,
            AdditionalMeasurements = additionalMeasurements,
            _displayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
        
        entry.ValidateInvariants();
        
        return entry;
    }
    
    public void UpdateMeasurements(
        decimal? chest = null,
        decimal? waist = null,
        decimal? hips = null,
        decimal? inseam = null,
        decimal? shoulder = null,
        decimal? length = null,
        decimal? width = null,
        decimal? height = null,
        decimal? weight = null)
    {
        if (chest.HasValue) Chest = chest;
        if (waist.HasValue) Waist = waist;
        if (hips.HasValue) Hips = hips;
        if (inseam.HasValue) Inseam = inseam;
        if (shoulder.HasValue) Shoulder = shoulder;
        if (length.HasValue) Length = length;
        if (width.HasValue) Width = width;
        if (height.HasValue) Height = height;
        if (weight.HasValue) Weight = weight;
        
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }
    
    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstNegative(newDisplayOrder, nameof(newDisplayOrder));
        _displayOrder = newDisplayOrder;
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
        if (Guid.Empty == SizeGuideId)
            throw new DomainException("Size guide ID boş olamaz");

        if (string.IsNullOrWhiteSpace(_sizeLabel))
            throw new DomainException("Beden etiketi boş olamaz");

        if (_sizeLabel.Length > 50)
            throw new DomainException("Beden etiketi en fazla 50 karakter olabilir");

        if (_displayOrder < 0)
            throw new DomainException("Görüntüleme sırası negatif olamaz");
    }
}

