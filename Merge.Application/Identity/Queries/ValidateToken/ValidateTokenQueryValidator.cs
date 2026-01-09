using FluentValidation;

namespace Merge.Application.Identity.Queries.ValidateToken;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class ValidateTokenQueryValidator : AbstractValidator<ValidateTokenQuery>
{
    public ValidateTokenQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token zorunludur.");
    }
}

