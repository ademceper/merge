using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQuestion;

public class GetQuestionQueryValidator : AbstractValidator<GetQuestionQuery>
{
    public GetQuestionQueryValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
