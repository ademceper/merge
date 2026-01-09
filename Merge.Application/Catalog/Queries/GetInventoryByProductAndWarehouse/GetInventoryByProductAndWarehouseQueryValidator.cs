using FluentValidation;

namespace Merge.Application.Catalog.Queries.GetInventoryByProductAndWarehouse;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetInventoryByProductAndWarehouseQueryValidator : AbstractValidator<GetInventoryByProductAndWarehouseQuery>
{
    public GetInventoryByProductAndWarehouseQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Depo ID'si zorunludur.");
    }
}

