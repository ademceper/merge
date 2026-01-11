using FluentValidation;

namespace Merge.Application.Notification.Commands.UpdatePreference;

/// <summary>
/// Update Preference Command Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class UpdatePreferenceCommandValidator : AbstractValidator<UpdatePreferenceCommand>
{
    public UpdatePreferenceCommandValidator()
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

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Güncelleme bilgileri zorunludur.");
    }
}
