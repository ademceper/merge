using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetProductTranslation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductTranslationQuery(
    Guid ProductId,
    string LanguageCode) : IRequest<ProductTranslationDto?>;

