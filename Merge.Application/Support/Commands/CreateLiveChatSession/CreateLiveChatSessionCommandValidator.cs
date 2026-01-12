using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateLiveChatSession;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateLiveChatSessionCommandValidator : AbstractValidator<CreateLiveChatSessionCommand>
{
    public CreateLiveChatSessionCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        // UserId veya GuestName/GuestEmail zorunlu
        RuleFor(x => x)
            .Must(x => x.UserId.HasValue || (!string.IsNullOrEmpty(x.GuestName) && !string.IsNullOrEmpty(x.GuestEmail)))
            .WithMessage("Kullanıcı ID veya misafir adı ve e-posta adresi gereklidir");

        When(x => !string.IsNullOrEmpty(x.GuestName), () =>
        {
            RuleFor(x => x.GuestName)
                .MinimumLength(supportSettings.MinGuestNameLength).WithMessage($"Misafir adı en az {supportSettings.MinGuestNameLength} karakter olmalıdır")
                .MaximumLength(supportSettings.MaxGuestNameLength).WithMessage($"Misafir adı en fazla {supportSettings.MaxGuestNameLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.GuestEmail), () =>
        {
            RuleFor(x => x.GuestEmail)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
                .MaximumLength(supportSettings.MaxEmailLength).WithMessage($"E-posta adresi en fazla {supportSettings.MaxEmailLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Department), () =>
        {
            RuleFor(x => x.Department)
                .MaximumLength(supportSettings.MaxDepartmentLength).WithMessage($"Departman en fazla {supportSettings.MaxDepartmentLength} karakter olmalıdır");
        });
    }
}
