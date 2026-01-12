using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.International.Queries.GetStaticTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ⚠️ NOTE: Dictionary<string, string> burada kabul edilebilir çünkü key-value çiftleri dinamik ve güvenlik riski düşük
public record GetStaticTranslationsQuery(
    string LanguageCode,
    string? Category = null) : IRequest<Dictionary<string, string>>;

