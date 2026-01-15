using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Queries.IsEmailVerified;

public class IsEmailVerifiedQueryValidator : AbstractValidator<IsEmailVerifiedQuery>
{
    public IsEmailVerifiedQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");
    }
}

