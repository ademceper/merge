using FluentValidation;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductQuestions;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetProductQuestionsQueryValidator : AbstractValidator<GetProductQuestionsQuery>
{
    public GetProductQuestionsQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}
