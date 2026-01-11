using FluentValidation;

namespace Merge.Application.Notification.Commands.DeletePreference;

/// <summary>
/// Delete Preference Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
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
