using MediatR;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SetUserCurrencyPreferenceCommand(
    Guid UserId,
    string CurrencyCode) : IRequest<Unit>;

