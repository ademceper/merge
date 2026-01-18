using FluentValidation;

namespace Merge.Application.Marketing.Queries.CalculateGiftCardDiscount;

public class CalculateGiftCardDiscountQueryValidator : AbstractValidator<CalculateGiftCardDiscountQuery>
{
    public CalculateGiftCardDiscountQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Hediye kartı kodu zorunludur.")
            .MaximumLength(50).WithMessage("Hediye kartı kodu en fazla 50 karakter olabilir.");

        RuleFor(x => x.OrderAmount)
            .GreaterThan(0).WithMessage("Sipariş tutarı 0'dan büyük olmalıdır.");
    }
}
