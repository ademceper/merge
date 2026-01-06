using FluentValidation;

namespace Merge.Application.Analytics.Commands.DeleteUser;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

