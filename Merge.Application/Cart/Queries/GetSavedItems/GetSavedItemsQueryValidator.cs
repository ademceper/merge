using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetSavedItems;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetSavedItemsQueryValidator : AbstractValidator<GetSavedItemsQuery>
{
    public GetSavedItemsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Sayfa numarası 0'dan büyük olmalıdır");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Sayfa boyutu 0'dan büyük olmalıdır");
    }
}

