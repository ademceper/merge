using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Queries.GetStaticTranslations;

// ⚠️ NOTE: Dictionary<string, string> burada kabul edilebilir çünkü key-value çiftleri dinamik ve güvenlik riski düşük
public record GetStaticTranslationsQuery(
    string LanguageCode,
    string? Category = null) : IRequest<Dictionary<string, string>>;

