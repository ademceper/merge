using FluentValidation;

namespace Merge.Application.Notification.Queries.IsNotificationEnabled;

/// <summary>
/// Is Notification Enabled Query Validator - BOLUM 2.1: FluentValidation (ZORUNLU)
/// </summary>
public class IsNotificationEnabledQueryValidator : AbstractValidator<IsNotificationEnabledQuery>
{
    public IsNotificationEnabledQueryValidator()
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
