using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.CreatePreference;


public class CreatePreferenceCommandValidator : AbstractValidator<CreatePreferenceCommand>
{
    public CreatePreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage("Tercih bilgileri zorunludur.");

        When(x => x.Dto != null, () =>
        {
            RuleFor(x => x.Dto.NotificationType)
                .IsInEnum()
                .WithMessage("Geçerli bir bildirim tipi seçiniz.");

            RuleFor(x => x.Dto.Channel)
                .IsInEnum()
                .WithMessage("Geçerli bir kanal seçiniz.");
        });
    }
}
