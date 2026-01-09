using FluentValidation;

namespace Merge.Application.International.Commands.SyncExchangeRates;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class SyncExchangeRatesCommandValidator : AbstractValidator<SyncExchangeRatesCommand>
{
    public SyncExchangeRatesCommandValidator()
    {
        // SyncExchangeRatesCommand parametre almadığı için validator boş
        // Ancak FluentValidation pipeline'ı için gerekli
    }
}

