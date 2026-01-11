using FluentValidation;

namespace Merge.Application.Product.Commands.DeleteAnswer;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteAnswerCommandValidator : AbstractValidator<DeleteAnswerCommand>
{
    public DeleteAnswerCommandValidator()
    {
        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("Cevap ID boş olamaz.");
    }
}
