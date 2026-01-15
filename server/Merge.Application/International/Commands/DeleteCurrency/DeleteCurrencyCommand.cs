using MediatR;

namespace Merge.Application.International.Commands.DeleteCurrency;

public record DeleteCurrencyCommand(Guid Id) : IRequest<Unit>;

