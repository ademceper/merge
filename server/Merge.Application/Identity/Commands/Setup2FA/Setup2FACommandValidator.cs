using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.Setup2FA;

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

