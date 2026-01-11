using FluentValidation;

namespace Merge.Application.Product.Commands.MarkAnswerHelpful;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class MarkAnswerHelpfulCommandValidator : AbstractValidator<MarkAnswerHelpfulCommand>
{
    public MarkAnswerHelpfulCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("Cevap ID boş olamaz.");
    }
}
