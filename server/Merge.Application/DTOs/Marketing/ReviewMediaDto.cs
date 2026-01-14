using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Review Media DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record ReviewMediaDto(
    Guid Id,
    string MediaType,
    string Url,
    string ThumbnailUrl);
