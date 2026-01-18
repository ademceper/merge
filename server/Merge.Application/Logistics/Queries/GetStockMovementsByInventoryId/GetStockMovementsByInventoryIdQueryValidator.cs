using FluentValidation;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;

public class GetStockMovementsByInventoryIdQueryValidator : AbstractValidator<GetStockMovementsByInventoryIdQuery>
{
    public GetStockMovementsByInventoryIdQueryValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty().WithMessage("Envanter ID'si zorunludur.");
    }
}

