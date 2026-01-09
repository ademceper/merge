using FluentValidation;

namespace Merge.Application.Identity.Commands.RevokeToken;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token zorunludur.");
    }
}

