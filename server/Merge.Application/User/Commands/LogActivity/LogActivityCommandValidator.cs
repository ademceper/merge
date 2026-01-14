using FluentValidation;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.LogActivity;

public class LogActivityCommandValidator : AbstractValidator<LogActivityCommand>
{
    public LogActivityCommandValidator()
    {
        RuleFor(x => x.ActivityType)
            .NotEmpty()
            .WithMessage("Aktivite tipi zorunludur.")
            .MaximumLength(100)
            .WithMessage("Aktivite tipi en fazla 100 karakter olabilir.");

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("Entity tipi zorunludur.")
            .MaximumLength(100)
            .WithMessage("Entity tipi en fazla 100 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Açıklama zorunludur.")
            .MaximumLength(2000)
            .WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.IpAddress)
            .NotEmpty()
            .WithMessage("IP adresi zorunludur.");

        RuleFor(x => x.UserAgent)
            .NotEmpty()
            .WithMessage("User agent zorunludur.");

        RuleFor(x => x.DurationMs)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Süre negatif olamaz.");

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.ErrorMessage))
            .WithMessage("Hata mesajı en fazla 1000 karakter olabilir.");
    }
}
