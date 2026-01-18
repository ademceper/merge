using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AskQuestion;

public class AskQuestionCommandValidator : AbstractValidator<AskQuestionCommand>
{
    public AskQuestionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Soru boş olamaz.")
            .MinimumLength(5).WithMessage("Soru en az 5 karakter olmalıdır.")
            .MaximumLength(1000).WithMessage("Soru en fazla 1000 karakter olmalıdır.");
    }
}
