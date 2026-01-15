using MediatR;

namespace Merge.Application.International.Commands.BulkCreateStaticTranslations;

public record BulkCreateStaticTranslationsCommand(
    string LanguageCode,
    Dictionary<string, string> Translations) : IRequest<Unit>;

