namespace Merge.Domain.Entities;

public class SizeGuide : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Men's Shirt Sizes", "Women's Shoe Sizes"
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public string? Brand { get; set; } // Brand-specific size guide
    public SizeGuideType Type { get; set; } = SizeGuideType.Standard;
    public string MeasurementUnit { get; set; } = "cm"; // cm, inch, etc.
    public bool IsActive { get; set; } = true;
    public ICollection<SizeGuideEntry> Entries { get; set; } = new List<SizeGuideEntry>();
    public ICollection<ProductSizeGuide> ProductSizeGuides { get; set; } = new List<ProductSizeGuide>();
}

public class SizeGuideEntry : BaseEntity
{
    public Guid SizeGuideId { get; set; }
    public SizeGuide SizeGuide { get; set; } = null!;
    public string SizeLabel { get; set; } = string.Empty; // XS, S, M, L, XL, 38, 40, etc.
    public string? AlternativeLabel { get; set; } // US 8, EU 38, UK 10
    public decimal? Chest { get; set; }
    public decimal? Waist { get; set; }
    public decimal? Hips { get; set; }
    public decimal? Inseam { get; set; }
    public decimal? Shoulder { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; } // For height-based sizing
    public decimal? Weight { get; set; } // For weight-based sizing
    public string? AdditionalMeasurements { get; set; } // JSON for custom measurements
    public int DisplayOrder { get; set; } = 0;
}

public class ProductSizeGuide : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid SizeGuideId { get; set; }
    public SizeGuide SizeGuide { get; set; } = null!;
    public string? CustomNotes { get; set; } // Product-specific sizing notes
    public bool FitType { get; set; } = true; // true = Regular Fit, false = Slim Fit, etc.
    public string? FitDescription { get; set; }
}

public class VirtualTryOn : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    public string ModelUrl { get; set; } = string.Empty; // AR/3D model URL
    public string? PreviewImageUrl { get; set; } // Preview image URL
    public string ViewerType { get; set; } = "AR"; // AR, 3D, Image
    public string? Configuration { get; set; } // JSON configuration for viewer
    public decimal? Height { get; set; } // User height for virtual try-on
    public decimal? Chest { get; set; } // User chest measurement
    public decimal? Waist { get; set; } // User waist measurement
    public decimal? Hips { get; set; } // User hips measurement
}

public enum SizeGuideType
{
    Standard,      // General size chart
    Detailed,      // Detailed measurements
    Conversion,    // Size conversion chart (US/EU/UK)
    Custom         // Custom sizing system
}
