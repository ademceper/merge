using FluentValidation;

namespace Merge.Application.Catalog.Commands.ReserveStock;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Rezervasyon miktarı 1'den büyük olmalıdır.");
    }
}

