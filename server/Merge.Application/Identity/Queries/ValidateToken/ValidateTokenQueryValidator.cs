using FluentValidation;

namespace Merge.Application.Identity.Queries.ValidateToken;

public class ValidateTokenQueryValidator() : AbstractValidator<ValidateTokenQuery>
{
    public ValidateTokenQueryValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token zorunludur.");
    }
}

