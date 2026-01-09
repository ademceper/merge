using FluentValidation;

namespace Merge.Application.Identity.Commands.Setup2FA;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class Setup2FACommandValidator : AbstractValidator<Setup2FACommand>
{
    public Setup2FACommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.SetupDto)
            .NotNull().WithMessage("Setup DTO zorunludur.");
    }
}

