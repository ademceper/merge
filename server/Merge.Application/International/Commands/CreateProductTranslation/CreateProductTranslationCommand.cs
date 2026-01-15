using MediatR;
using Merge.Application.DTOs.International;

namespace Merge.Application.International.Commands.CreateProductTranslation;

public record CreateProductTranslationCommand(
    Guid ProductId,
    string LanguageCode,
    string Name,
    string Description,
    string ShortDescription,
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords) : IRequest<ProductTranslationDto>;

