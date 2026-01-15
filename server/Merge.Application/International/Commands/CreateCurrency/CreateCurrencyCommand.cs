using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.CreateCurrency;

public record CreateCurrencyCommand(
    string Code,
    string Name,
    string Symbol,
    decimal ExchangeRate,
    bool IsBaseCurrency,
    bool IsActive,
    int DecimalPlaces,
    string Format) : IRequest<CurrencyDto>;

