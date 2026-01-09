using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.UpdateCurrency;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCurrencyCommand(
    Guid Id,
    string Name,
    string Symbol,
    decimal ExchangeRate,
    bool IsActive,
    int DecimalPlaces,
    string Format) : IRequest<CurrencyDto>;

