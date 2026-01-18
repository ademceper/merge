using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.MarkReviewHelpfulness;

public record MarkReviewHelpfulnessCommand(
    Guid UserId,
    Guid ReviewId,
    bool IsHelpful
) : IRequest;
