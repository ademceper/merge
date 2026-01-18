using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateProductBadges;

public record EvaluateProductBadgesCommand(
    Guid ProductId
) : IRequest;
