using FluentValidation;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderByPONumber;

public class GetPurchaseOrderByPONumberQueryValidator : AbstractValidator<GetPurchaseOrderByPONumberQuery>
{
    public GetPurchaseOrderByPONumberQueryValidator()
    {
        RuleFor(x => x.PONumber)
            .NotEmpty().WithMessage("PO numarası boş olamaz");
    }
}

