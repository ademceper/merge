using FluentValidation;

namespace Merge.Application.Cart.Queries.GetCartEmailHistory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCartEmailHistoryQueryValidator : AbstractValidator<GetCartEmailHistoryQuery>
{
    public GetCartEmailHistoryQueryValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır");
    }
}

