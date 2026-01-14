using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByProductId;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetStockMovementsByProductIdQueryValidator : AbstractValidator<GetStockMovementsByProductIdQuery>
{
    public GetStockMovementsByProductIdQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}

