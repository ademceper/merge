using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.CreateCurrency;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCurrencyCommand(
    string Code,
    string Name,
    string Symbol,
    decimal ExchangeRate,
    bool IsBaseCurrency,
    bool IsActive,
    int DecimalPlaces,
    string Format) : IRequest<CurrencyDto>;

