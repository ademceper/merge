using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Queries.GetProductTranslation;

public record GetProductTranslationQuery(
    Guid ProductId,
    string LanguageCode) : IRequest<ProductTranslationDto?>;

