using FluentValidation;

namespace Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class RemoveProductFromFlashSaleCommandValidator : AbstractValidator<RemoveProductFromFlashSaleCommand>
{
    public RemoveProductFromFlashSaleCommandValidator()
    {
        RuleFor(x => x.FlashSaleId)
            .NotEmpty().WithMessage("Flash Sale ID'si zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");
    }
}
