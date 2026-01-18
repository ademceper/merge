using FluentValidation;

namespace Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;

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
