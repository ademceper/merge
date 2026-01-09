using MediatR;

namespace Merge.Application.International.Commands.DeleteCurrency;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteCurrencyCommand(Guid Id) : IRequest<Unit>;

