using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCategoryTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCategoryTranslationsQuery(Guid CategoryId) : IRequest<IEnumerable<CategoryTranslationDto>>;

