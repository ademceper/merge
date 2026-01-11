using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Review.Commands.AddReviewMedia;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
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
