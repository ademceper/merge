using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.MarkQuestionHelpful;

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
