using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.Verify2FACode;

public class Verify2FACodeCommandValidator : AbstractValidator<Verify2FACodeCommand>
{
    public Verify2FACodeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Kod zorunludur.")
            .MaximumLength(10).WithMessage("Kod en fazla 10 karakter olabilir.");
    }
}

