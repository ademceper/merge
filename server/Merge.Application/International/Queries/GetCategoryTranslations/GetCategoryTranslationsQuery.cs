using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetCategoryTranslations;

public record GetCategoryTranslationsQuery(Guid CategoryId) : IRequest<IEnumerable<CategoryTranslationDto>>;

