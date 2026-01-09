using MediatR;

namespace Merge.Application.International.Commands.SetUserLanguagePreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SetUserLanguagePreferenceCommand(
    Guid UserId,
    string LanguageCode) : IRequest<Unit>;

