using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Queries.Get2FAStatus;

public class Get2FAStatusQueryValidator : AbstractValidator<Get2FAStatusQuery>
{
    public Get2FAStatusQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID zorunludur.");
    }
}

