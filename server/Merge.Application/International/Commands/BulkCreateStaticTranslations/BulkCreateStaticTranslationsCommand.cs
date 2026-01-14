using MediatR;

namespace Merge.Application.International.Commands.BulkCreateStaticTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record BulkCreateStaticTranslationsCommand(
    string LanguageCode,
    Dictionary<string, string> Translations) : IRequest<Unit>;

