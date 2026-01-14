using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Content.Commands.UpdateBanner;

public record UpdateBannerCommand(
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
    Guid? ProductId
) : IRequest<BannerDto>;
