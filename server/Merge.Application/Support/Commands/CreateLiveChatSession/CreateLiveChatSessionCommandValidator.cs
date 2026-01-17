using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateLiveChatSession;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateLiveChatSessionCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<CreateLiveChatSessionCommand>
{
    private readonly SupportSettings config = settings.Value;

    public CreateLiveChatSessionCommandValidator() : this(Options.Create(new SupportSettings()))
    {
// UserId veya GuestName/GuestEmail zorunlu
        RuleFor(x => x)
            .Must(x => x.UserId.HasValue || (!string.IsNullOrEmpty(x.GuestName) && !string.IsNullOrEmpty(x.GuestEmail)))
            .WithMessage("Kullanıcı ID veya misafir adı ve e-posta adresi gereklidir");

        When(x => !string.IsNullOrEmpty(x.GuestName), () =>
        {
            RuleFor(x => x.GuestName)
                .MinimumLength(config.MinGuestNameLength).WithMessage($"Misafir adı en az {config.MinGuestNameLength} karakter olmalıdır")
                .MaximumLength(config.MaxGuestNameLength).WithMessage($"Misafir adı en fazla {config.MaxGuestNameLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.GuestEmail), () =>
        {
            RuleFor(x => x.GuestEmail)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
                .MaximumLength(config.MaxEmailLength).WithMessage($"E-posta adresi en fazla {config.MaxEmailLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Department), () =>
        {
            RuleFor(x => x.Department)
                .MaximumLength(config.MaxDepartmentLength).WithMessage($"Departman en fazla {config.MaxDepartmentLength} karakter olmalıdır");
        });
    }
}
