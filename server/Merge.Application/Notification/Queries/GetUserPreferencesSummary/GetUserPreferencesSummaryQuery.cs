using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetUserPreferencesSummary;


public record GetUserPreferencesSummaryQuery(Guid UserId) : IRequest<NotificationPreferenceSummaryDto>;
