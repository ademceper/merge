using FluentValidation;

namespace Merge.Application.Identity.Commands.Enable2FA;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class Enable2FACommandValidator : AbstractValidator<Enable2FACommand>
{
    public Enable2FACommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.EnableDto)
            .NotNull().WithMessage("Enable DTO zorunludur.");
    }
}

