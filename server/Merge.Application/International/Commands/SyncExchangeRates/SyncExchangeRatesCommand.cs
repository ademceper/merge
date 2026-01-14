using MediatR;

namespace Merge.Application.International.Commands.SyncExchangeRates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SyncExchangeRatesCommand() : IRequest<Unit>;

