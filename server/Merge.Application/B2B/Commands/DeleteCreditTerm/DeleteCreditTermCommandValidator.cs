using FluentValidation;

namespace Merge.Application.B2B.Commands.DeleteCreditTerm;

public class DeleteCreditTermCommandValidator : AbstractValidator<DeleteCreditTermCommand>
{
    public DeleteCreditTermCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kredi koşulu ID boş olamaz");
    }
}

