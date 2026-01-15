using MediatR;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

public record SetUserLanguagePreferenceCommand(
    Guid UserId,
    string LanguageCode) : IRequest<Unit>;

