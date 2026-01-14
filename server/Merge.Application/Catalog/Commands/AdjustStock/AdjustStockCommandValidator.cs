using FluentValidation;

namespace Merge.Application.Catalog.Commands.AdjustStock;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.InventoryId)
            .NotEmpty()
            .WithMessage("Envanter ID'si zorunludur.");

        RuleFor(x => x.QuantityChange)
            .NotEqual(0)
            .WithMessage("Miktar değişikliği 0'dan farklı olmalıdır.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notlar en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

