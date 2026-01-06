using FluentValidation;

namespace Merge.Application.Analytics.Commands.ActivateUser;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

