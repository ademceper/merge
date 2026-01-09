using FluentValidation;

namespace Merge.Application.Identity.Queries.IsEmailVerified;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class IsEmailVerifiedQueryValidator : AbstractValidator<IsEmailVerifiedQuery>
{
    public IsEmailVerifiedQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");
    }
}

