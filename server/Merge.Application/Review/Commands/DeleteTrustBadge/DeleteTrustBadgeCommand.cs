using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteTrustBadge;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteTrustBadgeCommand(
    Guid BadgeId
) : IRequest<bool>;
