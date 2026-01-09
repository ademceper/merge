using FluentValidation;

namespace Merge.Application.Identity.Queries.Get2FAStatus;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class Get2FAStatusQueryValidator : AbstractValidator<Get2FAStatusQuery>
{
    public Get2FAStatusQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");
    }
}

