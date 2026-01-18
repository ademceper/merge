using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateSellerBadges;

public record EvaluateSellerBadgesCommand(
    Guid SellerId
) : IRequest;
