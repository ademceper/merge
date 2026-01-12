using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UnmarkAnswerHelpful;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class UnmarkAnswerHelpfulCommandValidator : AbstractValidator<UnmarkAnswerHelpfulCommand>
{
    public UnmarkAnswerHelpfulCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.AnswerId)
            .NotEmpty().WithMessage("Cevap ID boş olamaz.");
    }
}
