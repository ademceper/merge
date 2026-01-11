using MediatR;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Notification.Queries.GetUserPreferencesSummary;

/// <summary>
/// Get User Preferences Summary Query - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record GetUserPreferencesSummaryQuery(Guid UserId) : IRequest<NotificationPreferenceSummaryDto>;
