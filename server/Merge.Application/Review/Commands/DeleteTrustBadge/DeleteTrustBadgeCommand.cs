using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteTrustBadge;

public record DeleteTrustBadgeCommand(
    Guid BadgeId
) : IRequest<bool>;
