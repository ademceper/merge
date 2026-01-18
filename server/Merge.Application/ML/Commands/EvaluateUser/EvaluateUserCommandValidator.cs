using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.ML.Commands.EvaluateUser;

public class EvaluateUserCommandValidator : AbstractValidator<EvaluateUserCommand>
{
    public EvaluateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
