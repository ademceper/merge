using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetNotificationById;

/// <summary>
/// Get Notification By Id Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class GetNotificationByIdQueryValidator : AbstractValidator<GetNotificationByIdQuery>
{
    public GetNotificationByIdQueryValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("Bildirim ID'si zorunludur.");
    }
}
