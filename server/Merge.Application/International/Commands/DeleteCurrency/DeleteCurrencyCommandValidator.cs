using FluentValidation;

namespace Merge.Application.International.Commands.DeleteCurrency;

public class DeleteCurrencyCommandValidator() : AbstractValidator<DeleteCurrencyCommand>
{
    public DeleteCurrencyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Para birimi ID zorunludur.");
    }
}

