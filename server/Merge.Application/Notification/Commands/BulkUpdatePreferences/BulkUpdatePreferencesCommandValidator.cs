using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.BulkUpdatePreferences;


public class BulkUpdatePreferencesCommandValidator : AbstractValidator<BulkUpdatePreferencesCommand>
{
    public BulkUpdatePreferencesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Toplu güncelleme bilgileri zorunludur.");

        When(x => x.Dto is not null, () =>
        {
            RuleFor(x => x.Dto.Preferences)
                .NotNull()
                .WithMessage("Tercih listesi zorunludur.")
                .NotEmpty()
                .WithMessage("En az bir tercih belirtilmelidir.")
                .Must(preferences => preferences.Count <= 100)
                .WithMessage("Bir seferde en fazla 100 tercih güncellenebilir.");

            RuleForEach(x => x.Dto.Preferences)
                .ChildRules(preference =>
                {
                    preference.RuleFor(p => p.NotificationType)
                        .IsInEnum()
                        .WithMessage("Geçerli bir bildirim tipi seçiniz.");

                    preference.RuleFor(p => p.Channel)
                        .IsInEnum()
                        .WithMessage("Geçerli bir kanal seçiniz.");
                });
        });
    }
}
