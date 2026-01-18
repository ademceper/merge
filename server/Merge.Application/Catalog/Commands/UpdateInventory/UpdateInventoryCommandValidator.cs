using FluentValidation;

namespace Merge.Application.Catalog.Commands.UpdateInventory;

public class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
{
    public UpdateInventoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");

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

