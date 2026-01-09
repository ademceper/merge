using FluentValidation;

namespace Merge.Application.Catalog.Commands.CreateInventory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class CreateInventoryCommandValidator : AbstractValidator<CreateInventoryCommand>
{
    public CreateInventoryCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Miktar 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Minimum stok seviyesi 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.MaximumStockLevel)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Maksimum stok seviyesi 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Birim maliyet 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .WithMessage("Konum en fazla 200 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Location));
    }
}

