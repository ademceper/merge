using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.AddReviewMedia;

public record AddReviewMediaCommand(
    Guid ReviewId,
    string Url,
    string MediaType,
    string? ThumbnailUrl = null,
    int FileSize = 0,
    int? Width = null,
    int? Height = null,
    int? Duration = null,
    int DisplayOrder = 0
) : IRequest<ReviewMediaDto>;
