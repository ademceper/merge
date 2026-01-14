using FluentValidation;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.MarkAsRead;

/// <summary>
/// Mark As Read Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
{
    public MarkAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("Bildirim ID'si zorunludur.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
