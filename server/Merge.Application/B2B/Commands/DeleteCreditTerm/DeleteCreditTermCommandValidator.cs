using FluentValidation;

namespace Merge.Application.B2B.Commands.DeleteCreditTerm;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteCreditTermCommandValidator : AbstractValidator<DeleteCreditTermCommand>
{
    public DeleteCreditTermCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kredi koşulu ID boş olamaz");
    }
}

