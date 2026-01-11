using FluentValidation;

namespace Merge.Application.Product.Queries.GetUserQuestions;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetUserQuestionsQueryValidator : AbstractValidator<GetUserQuestionsQuery>
{
    public GetUserQuestionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}
