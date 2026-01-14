using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetStockMovementById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetStockMovementByIdQueryValidator : AbstractValidator<GetStockMovementByIdQuery>
{
    public GetStockMovementByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Stok hareketi ID'si zorunludur.");
    }
}

