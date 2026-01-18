using FluentValidation;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQueryValidator : AbstractValidator<GetPurchaseOrderByIdQuery>
{
    public GetPurchaseOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Satın alma siparişi ID boş olamaz");
    }
}

