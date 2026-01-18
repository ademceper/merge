using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Marketing;


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
