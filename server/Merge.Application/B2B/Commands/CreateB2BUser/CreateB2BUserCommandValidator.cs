using FluentValidation;

namespace Merge.Application.B2B.Commands.CreateB2BUser;

public class CreateB2BUserCommandValidator : AbstractValidator<CreateB2BUserCommand>
{
    public CreateB2BUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organizasyon ID boş olamaz");
    }
}

