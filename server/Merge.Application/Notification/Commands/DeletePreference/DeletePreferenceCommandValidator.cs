using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.DeletePreference;


public class DeletePreferenceCommandValidator : AbstractValidator<DeletePreferenceCommand>
{
    public DeletePreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.NotificationType)
            .IsInEnum()
            .WithMessage("Geçerli bir bildirim tipi seçiniz.");

        RuleFor(x => x.Channel)
            .IsInEnum()
            .WithMessage("Geçerli bir kanal seçiniz.");
    }
}
