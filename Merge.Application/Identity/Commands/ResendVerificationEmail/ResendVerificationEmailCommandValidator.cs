using FluentValidation;

namespace Merge.Application.Identity.Commands.ResendVerificationEmail;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class ResendVerificationEmailCommandValidator : AbstractValidator<ResendVerificationEmailCommand>
{
    public ResendVerificationEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");
    }
}

