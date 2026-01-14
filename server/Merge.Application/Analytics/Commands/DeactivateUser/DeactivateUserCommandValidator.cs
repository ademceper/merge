using FluentValidation;

namespace Merge.Application.Analytics.Commands.DeactivateUser;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

