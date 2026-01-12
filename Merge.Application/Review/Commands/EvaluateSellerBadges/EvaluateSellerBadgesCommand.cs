using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateSellerBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluateSellerBadgesCommand(
    Guid SellerId
) : IRequest;
