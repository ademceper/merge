using MediatR;

namespace Merge.Application.International.Commands.SetUserCurrencyPreference;

public record SetUserCurrencyPreferenceCommand(
    Guid UserId,
    string CurrencyCode) : IRequest<Unit>;

