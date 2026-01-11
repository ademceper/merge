using FluentValidation;

namespace Merge.Application.Product.Commands.DeleteQuestion;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteQuestionCommandValidator : AbstractValidator<DeleteQuestionCommand>
{
    public DeleteQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
