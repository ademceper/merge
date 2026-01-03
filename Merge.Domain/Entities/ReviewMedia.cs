using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// ReviewMedia Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReviewMedia : BaseEntity
{
    public Guid ReviewId { get; set; }
    public ReviewMediaType MediaType { get; set; }
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public int FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Duration { get; set; } // For videos in seconds
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public Review Review { get; set; } = null!;
}

