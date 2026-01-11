using MediatR;

namespace Merge.Application.Review.Commands.DeleteReviewMedia;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteReviewMediaCommand(
    Guid MediaId
) : IRequest;
