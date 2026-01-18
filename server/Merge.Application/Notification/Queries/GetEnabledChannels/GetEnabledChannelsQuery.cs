using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetEnabledChannels;


public record GetEnabledChannelsQuery(
    Guid UserId,
    NotificationType NotificationType) : IRequest<IEnumerable<string>>;
