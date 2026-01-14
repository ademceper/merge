using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetAbandonedCartById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetAbandonedCartByIdQueryValidator : AbstractValidator<GetAbandonedCartByIdQuery>
{
    public GetAbandonedCartByIdQueryValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");
    }
}

