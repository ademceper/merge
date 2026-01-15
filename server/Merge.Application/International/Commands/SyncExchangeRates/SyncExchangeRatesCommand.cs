using MediatR;

namespace Merge.Application.International.Commands.SyncExchangeRates;

public record SyncExchangeRatesCommand() : IRequest<Unit>;

