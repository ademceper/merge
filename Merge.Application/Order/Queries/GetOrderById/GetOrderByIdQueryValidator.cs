using FluentValidation;

namespace Merge.Application.Order.Queries.GetOrderById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Sipariş ID'si zorunludur.");
    }
}
