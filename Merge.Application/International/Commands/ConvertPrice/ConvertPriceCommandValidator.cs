using FluentValidation;

namespace Merge.Application.International.Commands.ConvertPrice;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class ConvertPriceCommandValidator : AbstractValidator<ConvertPriceCommand>
{
    public ConvertPriceCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

        RuleFor(x => x.FromCurrency)
            .NotEmpty().WithMessage("Kaynak para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("Hedef para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");
    }
}

