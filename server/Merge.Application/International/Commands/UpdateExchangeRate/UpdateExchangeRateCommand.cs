using MediatR;

namespace Merge.Application.International.Commands.UpdateExchangeRate;

public record UpdateExchangeRateCommand(
    string CurrencyCode,
    decimal NewRate,
    string Source = "Manual") : IRequest<Unit>;

