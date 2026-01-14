using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Email Template DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record EmailTemplateDto(
    Guid Id,
    string Name,
    string Description,
    string Subject,
    string HtmlContent,
    string Type,
    bool IsActive,
    string? Thumbnail,
    List<string> Variables);
