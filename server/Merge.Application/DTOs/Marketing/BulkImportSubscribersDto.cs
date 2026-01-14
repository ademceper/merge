using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Bulk Import Subscribers DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record BulkImportSubscribersDto
{
    [Required(ErrorMessage = "Subscribers list is required")]
    [MinLength(1, ErrorMessage = "At least one subscriber is required")]
    public List<CreateEmailSubscriberDto> Subscribers { get; init; } = new();
}
