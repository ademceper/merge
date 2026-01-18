using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateAndAwardBadges;

public record EvaluateAndAwardBadgesCommand(
    Guid? SellerId = null
) : IRequest;
