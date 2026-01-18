using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Queries.GetPreference;


public class GetPreferenceQueryValidator : AbstractValidator<GetPreferenceQuery>
{
    public GetPreferenceQueryValidator()
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
