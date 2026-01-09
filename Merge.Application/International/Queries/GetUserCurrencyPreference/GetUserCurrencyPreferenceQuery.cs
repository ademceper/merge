using MediatR;

namespace Merge.Application.International.Queries.GetUserCurrencyPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserCurrencyPreferenceQuery(Guid UserId) : IRequest<string>;

