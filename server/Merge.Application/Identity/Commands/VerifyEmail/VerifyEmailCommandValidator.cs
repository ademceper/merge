using FluentValidation;

namespace Merge.Application.Identity.Commands.VerifyEmail;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token zorunludur.");
    }
}

