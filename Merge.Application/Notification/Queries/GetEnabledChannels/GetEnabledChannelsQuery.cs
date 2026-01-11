using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Notification.Queries.GetEnabledChannels;

/// <summary>
/// Get Enabled Channels Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetEnabledChannelsQuery(
    Guid UserId,
    NotificationType NotificationType) : IRequest<IEnumerable<string>>;
