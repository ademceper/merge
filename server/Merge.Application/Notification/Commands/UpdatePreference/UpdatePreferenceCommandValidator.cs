using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.UpdatePreference;


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
