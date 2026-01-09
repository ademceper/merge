using FluentValidation;

namespace Merge.Application.Identity.Commands.RefreshToken;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token zorunludur.");
    }
}

