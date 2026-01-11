using FluentValidation;

namespace Merge.Application.Product.Commands.AnswerQuestion;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class AnswerQuestionCommandValidator : AbstractValidator<AnswerQuestionCommand>
{
    public AnswerQuestionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Cevap boş olamaz.")
            .MinimumLength(5).WithMessage("Cevap en az 5 karakter olmalıdır.")
            .MaximumLength(2000).WithMessage("Cevap en fazla 2000 karakter olmalıdır.");
    }
}
