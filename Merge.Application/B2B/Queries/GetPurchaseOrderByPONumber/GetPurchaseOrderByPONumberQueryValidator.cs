using FluentValidation;

namespace Merge.Application.B2B.Queries.GetPurchaseOrderByPONumber;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetPurchaseOrderByPONumberQueryValidator : AbstractValidator<GetPurchaseOrderByPONumberQuery>
{
    public GetPurchaseOrderByPONumberQueryValidator()
    {
        RuleFor(x => x.PONumber)
            .NotEmpty().WithMessage("PO numarası boş olamaz");
    }
}

