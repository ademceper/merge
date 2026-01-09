using FluentValidation;

namespace Merge.Application.LiveCommerce.Queries.GetStreamsBySeller;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetStreamsBySellerQueryValidator : AbstractValidator<GetStreamsBySellerQuery>
{
    public GetStreamsBySellerQueryValidator()
    {
        RuleFor(x => x.SellerId)
            .NotEmpty().WithMessage("Satıcı ID'si zorunludur.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Sayfa boyutu en fazla 100 olabilir.");
    }
}

