using FluentValidation;

namespace Merge.Application.Notification.Commands.DeleteNotification;

/// <summary>
/// Delete Notification Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class DeleteNotificationCommandValidator : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("Bildirim ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
