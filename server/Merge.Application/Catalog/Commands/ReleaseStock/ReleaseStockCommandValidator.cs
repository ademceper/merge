using FluentValidation;

namespace Merge.Application.Catalog.Commands.ReleaseStock;

public class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Serbest bırakma miktarı 1'den büyük olmalıdır.");
    }
}

