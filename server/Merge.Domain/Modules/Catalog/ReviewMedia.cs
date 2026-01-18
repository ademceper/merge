using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Catalog;

/// <summary>
/// ReviewMedia Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class ReviewMedia : BaseEntity
{
    public Guid ReviewId { get; private set; }
    public ReviewMediaType MediaType { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public int FileSize { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public int? Duration { get; private set; } // For videos in seconds
    public int DisplayOrder { get; private set; } = 0;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Review Review { get; private set; } = null!;

    private ReviewMedia() { }

    public static ReviewMedia Create(
        Guid reviewId,
        ReviewMediaType mediaType,
        string url,
        string? thumbnailUrl = null,
        int fileSize = 0,
        int? width = null,
        int? height = null,
        int? duration = null,
        int displayOrder = 0)
    {
        Guard.AgainstDefault(reviewId, nameof(reviewId));
        Guard.AgainstNullOrEmpty(url, nameof(url));
        Guard.AgainstOutOfRange(fileSize, 0, int.MaxValue, nameof(fileSize));
        Guard.AgainstOutOfRange(displayOrder, 0, int.MaxValue, nameof(displayOrder));

        var media = new ReviewMedia
        {
            Id = Guid.NewGuid(),
            ReviewId = reviewId,
            MediaType = mediaType,
            Url = url,
            ThumbnailUrl = thumbnailUrl ?? url,
            FileSize = fileSize,
            Width = width,
            Height = height,
            Duration = duration,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };
        
        media.ValidateInvariants();
        
        return media;
    }

    public void UpdateUrl(string newUrl)
    {
        Guard.AgainstNullOrEmpty(newUrl, nameof(newUrl));
        Url = newUrl;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void UpdateThumbnailUrl(string newThumbnailUrl)
    {
        ThumbnailUrl = newThumbnailUrl ?? Url;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void UpdateDisplayOrder(int newDisplayOrder)
    {
        Guard.AgainstOutOfRange(newDisplayOrder, 0, int.MaxValue, nameof(newDisplayOrder));
        DisplayOrder = newDisplayOrder;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        ValidateInvariants();
    }

    private void ValidateInvariants()
    {
        if (Guid.Empty == ReviewId)
            throw new DomainException("Review ID boş olamaz");

        if (string.IsNullOrWhiteSpace(Url))
            throw new DomainException("Media URL boş olamaz");

        if (FileSize < 0)
            throw new DomainException("Dosya boyutu negatif olamaz");

        if (DisplayOrder < 0)
            throw new DomainException("Görüntüleme sırası negatif olamaz");
    }
}

