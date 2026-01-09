using FluentValidation;

namespace Merge.Application.International.Commands.UpdateExchangeRate;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateExchangeRateCommandValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateCommandValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.NewRate)
            .GreaterThanOrEqualTo(0).WithMessage("Döviz kuru 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.Source)
            .MaximumLength(50).WithMessage("Kaynak en fazla 50 karakter olabilir.");
    }
}

