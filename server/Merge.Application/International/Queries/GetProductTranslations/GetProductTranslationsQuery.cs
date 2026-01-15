using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetProductTranslations;

public record GetProductTranslationsQuery(Guid ProductId) : IRequest<IEnumerable<ProductTranslationDto>>;

