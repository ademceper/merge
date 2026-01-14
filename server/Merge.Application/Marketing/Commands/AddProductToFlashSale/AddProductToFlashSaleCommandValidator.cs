using FluentValidation;

namespace Merge.Application.Marketing.Commands.AddProductToFlashSale;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class AddProductToFlashSaleCommandValidator : AbstractValidator<AddProductToFlashSaleCommand>
{
    public AddProductToFlashSaleCommandValidator()
    {
        RuleFor(x => x.FlashSaleId)
            .NotEmpty().WithMessage("Flash Sale ID'si zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.SalePrice)
            .GreaterThan(0).WithMessage("Satış fiyatı 0'dan büyük olmalıdır.");

        RuleFor(x => x.StockLimit)
            .GreaterThan(0).WithMessage("Stok limiti 0'dan büyük olmalıdır.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sıralama değeri negatif olamaz.");
    }
}
