using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UnmarkQuestionHelpful;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UnmarkQuestionHelpfulCommandValidator : AbstractValidator<UnmarkQuestionHelpfulCommand>
{
    public UnmarkQuestionHelpfulCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
