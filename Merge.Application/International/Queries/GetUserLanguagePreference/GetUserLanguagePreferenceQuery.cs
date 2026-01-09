using MediatR;

namespace Merge.Application.International.Queries.GetUserLanguagePreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserLanguagePreferenceQuery(Guid UserId) : IRequest<string>;

