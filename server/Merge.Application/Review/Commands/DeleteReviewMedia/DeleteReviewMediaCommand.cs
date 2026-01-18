using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteReviewMedia;

public record DeleteReviewMediaCommand(
    Guid MediaId
) : IRequest;
