using FluentValidation;

namespace Merge.Application.International.Commands.SyncExchangeRates;

public class SyncExchangeRatesCommandValidator : AbstractValidator<SyncExchangeRatesCommand>
{
    public SyncExchangeRatesCommandValidator()
    {
        // SyncExchangeRatesCommand parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

