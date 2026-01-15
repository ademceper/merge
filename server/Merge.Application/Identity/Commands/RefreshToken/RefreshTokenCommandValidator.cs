using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.RefreshToken;

public class RefreshTokenCommandValidator() : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token zorunludur.");
    }
}

