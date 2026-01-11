using FluentValidation;

namespace Merge.Application.Notification.Queries.GetPreference;

/// <summary>
/// Get Preference Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
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
