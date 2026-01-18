using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Marketing;


public record ReviewMediaDto(
    Guid Id,
    string MediaType,
    string Url,
    string ThumbnailUrl);
