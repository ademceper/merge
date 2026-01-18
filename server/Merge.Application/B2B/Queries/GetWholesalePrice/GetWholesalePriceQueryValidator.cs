using FluentValidation;

namespace Merge.Application.B2B.Queries.GetWholesalePrice;

public class GetWholesalePriceQueryValidator : AbstractValidator<GetWholesalePriceQuery>
{
    public GetWholesalePriceQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID boş olamaz");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır");
    }
}

