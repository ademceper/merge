using MediatR;

namespace Merge.Application.International.Commands.UpdateExchangeRate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateExchangeRateCommand(
    string CurrencyCode,
    decimal NewRate,
    string Source = "Manual") : IRequest<Unit>;

