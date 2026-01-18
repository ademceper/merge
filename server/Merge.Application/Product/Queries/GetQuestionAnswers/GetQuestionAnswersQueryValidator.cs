using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQuestionAnswers;

public class GetQuestionAnswersQueryValidator : AbstractValidator<GetQuestionAnswersQuery>
{
    public GetQuestionAnswersQueryValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
