using Merge.Domain.Modules.Content;
namespace Merge.Application.DTOs.Marketing;


public record BannerDto(
    Guid Id,
    string Title,
    string? Description,
    string ImageUrl,
    string? LinkUrl,
    string Position,
    int SortOrder,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? CategoryId,
    Guid? ProductId)
{
    public bool IsAvailable => IsActive &&
        (!StartDate.HasValue || DateTime.UtcNow >= StartDate.Value) &&
        (!EndDate.HasValue || DateTime.UtcNow <= EndDate.Value);
}
