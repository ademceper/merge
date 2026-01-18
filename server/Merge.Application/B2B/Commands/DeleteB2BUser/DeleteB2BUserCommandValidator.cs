using FluentValidation;

namespace Merge.Application.B2B.Commands.DeleteB2BUser;

public class DeleteB2BUserCommandValidator : AbstractValidator<DeleteB2BUserCommand>
{
    public DeleteB2BUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("B2B kullanıcı ID boş olamaz");
    }
}

