using FluentValidation;

namespace Merge.Application.International.Commands.DeleteCurrency;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
{
    public DeleteCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Para birimi ID zorunludur.");
    }
}

