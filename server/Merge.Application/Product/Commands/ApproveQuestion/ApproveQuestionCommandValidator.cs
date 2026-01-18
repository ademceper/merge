using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ApproveQuestion;

public class ApproveQuestionCommandValidator : AbstractValidator<ApproveQuestionCommand>
{
    public ApproveQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
