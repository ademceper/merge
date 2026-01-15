using MediatR;

namespace Merge.Application.International.Queries.GetUserLanguagePreference;

public record GetUserLanguagePreferenceQuery(Guid UserId) : IRequest<string>;

