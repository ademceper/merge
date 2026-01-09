using FluentValidation;

namespace Merge.Application.International.Commands.CreateCurrency;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, 10).WithMessage("Para birimi kodu en az 3, en fazla 10 karakter olmalıdır.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Para birimi adı zorunludur.")
            .MaximumLength(100).WithMessage("Para birimi adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol zorunludur.")
            .MaximumLength(10).WithMessage("Sembol en fazla 10 karakter olabilir.");

        RuleFor(x => x.ExchangeRate)
            .GreaterThanOrEqualTo(0).WithMessage("Döviz kuru 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.DecimalPlaces)
            .InclusiveBetween(0, 10).WithMessage("Ondalık basamak sayısı 0 ile 10 arasında olmalıdır.");

        RuleFor(x => x.Format)
            .MaximumLength(50).WithMessage("Format en fazla 50 karakter olabilir.");
    }
}

