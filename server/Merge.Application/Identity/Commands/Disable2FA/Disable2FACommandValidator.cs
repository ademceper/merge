using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.Disable2FA;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class Disable2FACommandValidator : AbstractValidator<Disable2FACommand>
{
    public Disable2FACommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.DisableDto)
            .NotNull().WithMessage("Disable DTO zorunludur.");
    }
}

