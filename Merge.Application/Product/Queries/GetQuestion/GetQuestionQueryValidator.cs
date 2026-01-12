using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQuestion;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetQuestionQueryValidator : AbstractValidator<GetQuestionQuery>
{
    public GetQuestionQueryValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("Soru ID boş olamaz.");
    }
}
