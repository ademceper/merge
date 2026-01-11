using FluentValidation;

namespace Merge.Application.Product.Queries.GetUnansweredQuestions;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetUnansweredQuestionsQueryValidator : AbstractValidator<GetUnansweredQuestionsQuery>
{
    public GetUnansweredQuestionsQueryValidator()
    {
        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Limit en fazla 100 olabilir.");
    }
}
