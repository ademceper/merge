using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetProductTranslations;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductTranslationsQuery(Guid ProductId) : IRequest<IEnumerable<ProductTranslationDto>>;

