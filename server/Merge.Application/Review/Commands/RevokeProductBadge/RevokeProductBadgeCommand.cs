using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.RevokeProductBadge;

public record RevokeProductBadgeCommand(
    Guid ProductId,
    Guid BadgeId
) : IRequest<bool>;
