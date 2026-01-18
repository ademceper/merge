using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.MarkAnswerHelpful;

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
