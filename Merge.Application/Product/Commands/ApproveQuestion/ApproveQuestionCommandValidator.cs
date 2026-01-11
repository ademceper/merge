using FluentValidation;

namespace Merge.Application.Product.Commands.ApproveQuestion;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ApproveQuestionCommandValidator : AbstractValidator<ApproveQuestionCommand>
{
    public ApproveQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
