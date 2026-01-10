using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetStockMovementsByInventoryIdQueryValidator : AbstractValidator<GetStockMovementsByInventoryIdQuery>
{
    public GetStockMovementsByInventoryIdQueryValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty().WithMessage("Envanter ID'si zorunludur.");
    }
}

