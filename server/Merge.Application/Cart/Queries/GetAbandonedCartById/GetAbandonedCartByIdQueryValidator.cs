using FluentValidation;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetAbandonedCartById;

public class GetAbandonedCartByIdQueryValidator : AbstractValidator<GetAbandonedCartByIdQuery>
{
    public GetAbandonedCartByIdQueryValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Sepet ID zorunludur");
    }
}

