using FluentValidation;

namespace Merge.Application.Cart.Queries.GetPreOrder;

public class GetPreOrderQueryValidator : AbstractValidator<GetPreOrderQuery>
{
    public GetPreOrderQueryValidator()
    {
        RuleFor(x => x.PreOrderId)
            .NotEmpty().WithMessage("Ön sipariş ID zorunludur.");
    }
}

