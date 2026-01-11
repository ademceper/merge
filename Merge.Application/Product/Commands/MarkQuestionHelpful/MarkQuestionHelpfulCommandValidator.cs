using FluentValidation;

namespace Merge.Application.Product.Commands.MarkQuestionHelpful;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class MarkQuestionHelpfulCommandValidator : AbstractValidator<MarkQuestionHelpfulCommand>
{
    public MarkQuestionHelpfulCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
