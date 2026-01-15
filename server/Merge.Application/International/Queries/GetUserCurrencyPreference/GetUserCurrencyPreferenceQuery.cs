using MediatR;

namespace Merge.Application.International.Queries.GetUserCurrencyPreference;

public record GetUserCurrencyPreferenceQuery(Guid UserId) : IRequest<string>;

