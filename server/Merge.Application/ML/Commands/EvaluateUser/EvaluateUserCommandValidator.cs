using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.ML.Commands.EvaluateUser;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class EvaluateUserCommandValidator : AbstractValidator<EvaluateUserCommand>
{
    public EvaluateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
