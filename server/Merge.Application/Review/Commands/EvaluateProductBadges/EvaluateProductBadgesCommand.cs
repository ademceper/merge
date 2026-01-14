using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.EvaluateProductBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluateProductBadgesCommand(
    Guid ProductId
) : IRequest;
