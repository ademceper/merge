using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Identity;
namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// VirtualTryOn Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
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

