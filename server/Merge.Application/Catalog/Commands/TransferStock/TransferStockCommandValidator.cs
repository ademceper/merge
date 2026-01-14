using FluentValidation;

namespace Merge.Application.Catalog.Commands.TransferStock;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class TransferStockCommandValidator : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.FromWarehouseId)
            .NotEmpty()
            .WithMessage("Kaynak depo ID'si zorunludur.");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty()
            .WithMessage("Hedef depo ID'si zorunludur.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Transfer miktarı 1'den büyük olmalıdır.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notlar en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

